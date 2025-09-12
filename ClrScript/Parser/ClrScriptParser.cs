using ClrScript.Elements.Expressions;
using ClrScript.Lexer;
using ClrScript.Elements.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using ClrScript.Elements;

namespace ClrScript.Parser
{
    class ClrScriptParser
    {
        readonly IReadOnlyList<Token> _tokens;
        readonly ClrScriptCompilationSettings _settings;

        int _current;

        public ClrScriptParser(IReadOnlyList<Token> tokens,
            ClrScriptCompilationSettings settings) 
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

            if (matchAny(TokenType.Print))
            {
                if (!_settings.AllowPrintStatement)
                {
                    throw new ClrScriptCompileException("Print statement is not allowed in the current environment.", previous());
                }

                return printStatement();
            }

            if (isAssignmentStatement())
            {
                return assignmentStatement(true);
            }

            if (isPostfixUnaryStatement())
            {
                return postfixUnaryStatement(true);
            }

            return expressionStatement();
        }

        Stmt forStatement()
        {
            var startLoc = previous();
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
                // Could be an assignment or expression statement
                if (isAssignmentStatement())
                {
                    var assignTo = or();
                    consume(TokenType.Equal, "Expected '=' in assignment.");
                    var value = expression();
                    consume(TokenType.SemiColon, "Expected ';' after initializer.");
                    initializer = new AssignStmt(assignTo, value);
                }
                else
                {
                    initializer = expressionStatement();
                }
            }
            
            Expr condition = null;
            if (!check(TokenType.SemiColon))
            {
                condition = expression();
            }

            consume(TokenType.SemiColon, "Expected ';' after loop condition.");
            
            Stmt increment = null;

            if (!check(TokenType.RightParen))
            {
                if (isAssignmentStatement())
                {
                    increment = assignmentStatement(false);
                }
                else if (isPostfixUnaryStatement())
                {
                    increment = postfixUnaryStatement(false);
                }
                else
                {
                    throw new ClrScriptCompileException("Was expecting either an assignment or postfix unary.", peek());
                }
            }

            consume(TokenType.RightParen, "Expected ')' after for clauses.");
            
            var body = statement();
            
            return new ForStmt(startLoc, initializer, condition, increment, body);
        }

        Stmt printStatement()
        {
            var expr = expression();
            consumeSemiColon();

            return new PrintStmt(expr);
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

        bool isAssignmentStatement()
        {
            var current = _current;
            try
            {
                // Try to parse the left side of a potential assignment
                or(); // Parse up to assignment level
                return check(TokenType.Equal);
            }
            catch
            {
                return false;
            }
            finally
            {
                _current = current;
            }
        }

        Stmt assignmentStatement(bool requireSemiColon)
        {
            var assignTo = or();
            consume(TokenType.Equal, "Expected '=' in assignment.");
            var value = expression();

            if (requireSemiColon)
            {
                consumeSemiColon();
            }
            
            return new AssignStmt(assignTo, value);
        }

        bool isPostfixUnaryStatement()
        {
            var current = _current;
            try
            {
                var expr = or(); 
                return check(TokenType.Increment) || check(TokenType.Decrement);
            }
            catch
            {
                return false;
            }
            finally
            {
                _current = current;
            }
        }

        Stmt postfixUnaryStatement(bool requireSemiColon)
        {
            var left = or();
            var op = consume(check(TokenType.Increment) ? TokenType.Increment : TokenType.Decrement, 
                "Expected '++' or '--'.");

            if (requireSemiColon)
            {
                consumeSemiColon();
            }

            return new PostFixUnaryAssignStmt(left, op);
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
            if (isLambda())
            {
                return lambda();
            }

            return or();
        }

        bool isLambda()
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
                else if (matchAny(TokenType.LeftBracket))
                {
                    var leftBracket = previous();
                    var indexerExpr = expression();

                    consume(TokenType.RightBracket, "Expected ']' after expression.");
                    expr = new Indexer(leftBracket, indexerExpr);
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

                return new Literal(double.Parse(token.Value), token);
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

            if (matchAny(TokenType.LeftBracket))
            {
                return arrayLiteral();
            }

            if (matchAny(TokenType.Identifier))
            {
                var token = previous();
                return new MemberRootAccess(token);
            }

            throw new ClrScriptCompileException("Expected expression.", previous());
        }

        Expr arrayLiteral()
        {
            var open = previous();
            var contents = new List<Expr>();

            if (!check(TokenType.RightBracket))
            {
                do
                {
                    var initExpr = expression();
                    contents.Add(initExpr);
                } while (matchAny(TokenType.Comma) && !check(TokenType.RightBracket));
            }

            consume(TokenType.RightBracket, "Expected ']' after array literal.");

            if (!_settings.AllowUserArrayConstruction)
            {
                throw new ClrScriptCompileException("Array construction is not allowed in this environment.", open);
            }

            return new ArrayLiteral(open, contents);
        }

        Expr objectLiteral()
        {
            var open = previous();
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

            if (!_settings.AllowUserObjectConstruction)
            {
                throw new ClrScriptCompileException("Object construction is not allowed in this environment.", open);
            }

            return new ObjectLiteral(properties, open);
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

            throw new ClrScriptCompileException(message, previous());
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
