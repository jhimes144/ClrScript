using Clank.Elements.Expressions;
using Clank.Lexer;
using Clank.Elements.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using Clank.Elements;

namespace Clank.Parser
{
    class ClankParser
    {
        readonly IReadOnlyList<Token> _tokens;
        readonly ClankCompilationSettings _settings;

        int _current;

        public ClankParser(IReadOnlyList<Token> tokens,
            ClankCompilationSettings settings) 
        {
            _settings = settings;
            _tokens = tokens;
        }

        public IReadOnlyList<Stmt> Parse()
        {
            var statements = new List<Stmt>();

            while (!isAtEnd())
            {
                statements.Add(declaration());
            }

            return statements;
        }

        Block blockStatement()
        {
            var openBrace = previous(); // The '{' token that was just matched
            var statements = new List<Stmt>();

            while (!check(TokenType.RightBrace) && !isAtEnd())
            {
                statements.Add(declaration());
            }

            consume(TokenType.RightBrace, "Expected '}' after scope block.");
            return new Block(statements, openBrace);
        }

        Stmt statement()
        {
            if (matchAny(TokenType.For))
            {
                return forStatement();
            }

            if (matchAny(TokenType.While))
            {
                return whileStatement();
            }

            if (matchAny(TokenType.LeftBrace))
            {
                return blockStatement();
            }

            if (matchAny(TokenType.If))
            {
                return ifStatement();
            }

            if (matchAny(TokenType.Return))
            {
                return returnStatement();
            }

            return expressionStatement();
        }

        Stmt forStatement()
        {
            var startLoc = previous(); // The 'for' token that was just matched
            consume(TokenType.LeftParen, "Expected '(' after 'for'.");
            
            // Initializer
            Stmt initializer;
            if (matchAny(TokenType.SemiColon))
            {
                initializer = null;
            }
            else if (matchAny(TokenType.Var))
            {
                initializer = varDeclaration();
            }
            else
            {
                initializer = expressionStatement();
            }
            
            Expr condition = null;
            if (!check(TokenType.SemiColon))
            {
                condition = expression();
            }

            consume(TokenType.SemiColon, "Expected ';' after loop condition.");
            
            Expr increment = null;
            if (!check(TokenType.RightParen))
            {
                increment = expression();
            }

            consume(TokenType.RightParen, "Expected ')' after for clauses.");
            
            var body = statement();
            
            return new ForStmt(startLoc, initializer, condition, increment, body);
        }

        Stmt returnStatement()
        {
            var startLoc = previous(); // The 'return' token that was just matched
            var returnExpr = expression();
            consumeSemiColon();
            return new ReturnStmt(startLoc, returnExpr);
        }

        Stmt whileStatement()
        {
            var startLoc = previous(); // The 'while' token that was just matched
            consume(TokenType.LeftParen, "Expected '('.");
            var condition = expression();
            consume(TokenType.RightParen, "Expected ')'.");

            var body = statement();
            return new WhileStmt(startLoc, condition, body);
        }

        Stmt ifStatement()
        {
            var startLoc = previous();
            consume(TokenType.LeftParen, "Expected '('.");
            var condition = expression();
            consume(TokenType.RightParen, "Expected ')'.");

            var thenBranch = statement();
            Stmt elseBranch = null;

            if (matchAny(TokenType.Else))
            {
                if (check(TokenType.If))
                {
                    elseBranch = ifStatement();
                }
                else
                {
                    elseBranch = statement();
                }
            }
            
            return new IfStmt(startLoc, condition, thenBranch, elseBranch);
        }

        Stmt declaration()
        {
            try
            {
                if (matchAny(TokenType.Var))
                {
                    return varDeclaration();
                }

                return statement();
            }
            catch (Exception)
            {

                throw;
            }
        }

        Stmt varDeclaration()
        {
            var startLoc = previous(); // The 'var' token that was just matched
            var name = consume(TokenType.Identifier, "Expected variable name.");

            Expr init = null;

            if (matchAny(TokenType.Equal))
            {
                init = expression();
            }

            consumeSemiColon();
            return new VarStmt(startLoc, VariableType.Var, name, init);
        }

        Stmt expressionStatement()
        {
            var expr = expression();
            consumeSemiColon();
            return new ExpressionStmt(expr);
        }

        Expr expression()
        {
            return assignment();
        }

        Expr assignment()
        {
            if (isProbablyLambda())
            {
                return lambda();
            }

            var expr = or();

            if (matchAny(TokenType.Equal))
            {
                var equals = previous();
                var value = assignment();

                if (expr is MemberRootAccess variable)
                {
                    return new Assign(variable.Name, value);
                }

                throw new ClankCompileException("Invalid assignment target.", equals);
            }

            return expr;
        }

        bool isProbablyLambda()
        {
            var current = _current;

            try
            {
                if (matchAny(TokenType.LeftParen))
                {
                    // Skip parameters
                    while (!check(TokenType.RightParen) && !isAtEnd())
                    {
                        if (matchAny(TokenType.Identifier))
                        {
                            if (matchAny(TokenType.Comma))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }

                    if (matchAny(TokenType.RightParen))
                    {
                        return check(TokenType.Arrow);
                    }
                }

                // identifier => ...
                else if (matchAny(TokenType.Identifier))
                {
                    return check(TokenType.Arrow);
                }

                return false;
            }
            finally
            {
                _current = current;
            }
        }

        Expr lambda()
        {
            var parameters = new List<Token>();

            if (matchAny(TokenType.LeftParen))
            {
                // Multi-parameter lambda: (a, b) => ...
                if (!check(TokenType.RightParen))
                {
                    do
                    {
                        parameters.Add(consume(TokenType.Identifier, "Expected parameter name."));
                    } while (matchAny(TokenType.Comma));
                }

                consume(TokenType.RightParen, "Expected ')' after lambda parameters.");
            }
            else if (check(TokenType.Identifier))
            {
                // Single parameter lambda: a => ... (no parentheses needed)
                parameters.Add(advance());
            }

            consume(TokenType.Arrow, "Expected '=>' in lambda expression.");

            Expr body;
            if (check(TokenType.LeftBrace))
            {
                body = new BlockExpr(blockStatement());
            }
            else
            {
                // Expression body: () => x + 1
                body = expression();
            }

            return new Lambda(parameters, body);
        }

        Expr or()
        {
            var expr = and();

            while (matchAny(TokenType.DoublePipe))
            {
                var op = previous();
                var right = and();
                expr = new Logical(expr, op, right);
            }

            return expr;
        }

        Expr and()
        {
            var expr = equality();

            while (matchAny(TokenType.DoubleAmpersand))
            {
                var op = previous();
                var right = equality();
                expr = new Logical(expr, op, right);
            }

            return expr;
        }

        Expr equality()
        {
            var expr = comparison();

            while (matchAny(TokenType.EqualEqual, TokenType.BangEqual))
            {
                var op = previous();
                var right = comparison();
                expr = new Binary(expr, op, right);
            }

            return expr;
        }

        Expr comparison()
        {
            var expr = term();

            while (matchAny(TokenType.GreaterThan, TokenType.GreaterThanOrEqual,
                TokenType.LessThan, TokenType.LessThanOrEqual))
            {
                var op = previous();
                var right = term();
                expr = new Binary(expr, op, right);
            }

            return expr;
        }

        Expr term()
        {
            var expr = factor();

            while (matchAny(TokenType.Minus, TokenType.Plus))
            {
                var op = previous();
                var right = factor();
                expr = new Binary(expr, op, right);
            }

            return expr;
        }

        Expr factor()
        {
            var expr = unary();
             
            while (matchAny(TokenType.Divide, TokenType.Multiply))
            { 
                var op = previous();
                var right = unary();
                expr = new Binary(expr, op, right);
            }

            return expr;
        }

        Expr unary()
        {
            if (matchAny(TokenType.Bang, TokenType.Minus))
            {
                var op = previous();
                var right = unary();
                return new Unary(op, right);
            }

            return postFix();
        }

        Expr postFix()
        {
            var expr = primary();

            while (true)
            {
                if (matchAny(TokenType.Dot))
                {
                    var name = consume(TokenType.Identifier, "Expected property name after '.'.");
                    expr = new MemberAccess(name, expr);
                }
                else if (matchAny(TokenType.LeftParen))
                {
                    var arguments = new List<Expr>();

                    if (!check(TokenType.RightParen))
                    {
                        do
                        {
                            arguments.Add(expression());
                        } while (matchAny(TokenType.Comma));
                    }

                    var paren = consume(TokenType.RightParen, "Expected ')' after arguments.");
                    expr = new Call(paren, expr, arguments);
                }
                else
                {
                    break;
                }
            }

            return expr;
        }

        Expr primary()
        {
            if (matchAny(TokenType.True))
            {
                return new Literal(true, previous());
            }

            if (matchAny(TokenType.False))
            {
                return new Literal(false, previous());
            }

            if (matchAny(TokenType.Null))
            {
                return new Literal(null, previous());
            }

            if (matchAny(TokenType.Number))
            {
                var token = previous();

                if (_settings.NumberPrecision == NumberPrecision.SinglePrecision)
                {
                    return new Literal(float.Parse(token.Value), token);
                }
                else if (_settings.NumberPrecision == NumberPrecision.DoublePrecision)
                {
                    return new Literal(double.Parse(token.Value), token);
                }
            }

            if (matchAny(TokenType.String))
            {
                var token = previous();
                return new Literal(token.Value.Trim('"'), token);
            }

            if (matchAny(TokenType.LeftParen))
            {
                var expr = expression();
                consume(TokenType.RightParen, "Expected ')'");
                return new Grouping(expr);
            }

            if (matchAny(TokenType.LeftBrace))
            {
                return objectLiteral();
            }

            if (matchAny(TokenType.Identifier))
            {
                var token = previous();
                return new MemberRootAccess(token);
            }

            throw new ClankCompileException("Expected expression.", previous());
        }

        Expr objectLiteral()
        {
            var openBrace = previous(); // The '{' token that was just matched
            var properties = new List<(Token Key, Expr Value)>();

            if (!check(TokenType.RightBrace))
            {
                do
                {
                    var key = consume(TokenType.Identifier, "Expected property name.");
                    consume(TokenType.Colon, "Expected ':' after property name.");
                    var value = expression();

                    properties.Add((key, value));

                } while (matchAny(TokenType.Comma) && !check(TokenType.RightBrace));
            }

            consume(TokenType.RightBrace, "Expected '}' after object literal.");
            return new ObjectLiteral(properties, openBrace);
        }

        bool matchAny(params TokenType[] tokenTypes)
        {
            foreach (var tokenType in tokenTypes)
            {
                if (check(tokenType))
                {
                    advance();
                    return true;
                }
            }

            return false;
        }

        Token consumeSemiColon()
        {
            return consume(TokenType.SemiColon, "Expected ';' after expression.");
        }

        Token consume(TokenType type, string message)
        {
            if (check(type))
            {
                return advance();
            }

            throw new ClankCompileException(message, previous());
        }

        bool check(TokenType type)
        {
            if (isAtEnd())
            {
                return false;
            }

            return peek().Type == type;
        }

        Token advance()
        {
            if (!isAtEnd())
            {
                _current++;
            }

            return previous();
        }

        bool isAtEnd()
        {
            return peek().Type == TokenType.EOF;
        }

        Token peek()
        {
            return _tokens[_current];
        }

        Token previous()
        {
            return _tokens[_current - 1];
        }
    }
}
