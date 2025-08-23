
using Clank;
using System;

string testScript = "for (var i = 1; i <= 5; i = i + 1)\r\n{\r\n\tprint \"hello\";\r\n}";

Console.WriteLine("Compiling and running test script...");
var context = ClankContext<Test>.Compile(testScript, null);

var result = context.Run(new Test());
Console.WriteLine($"Result: {result}");


public class Test : IImplementsPrintStmt
{
    public void Print(object obj)
    {
        Console.WriteLine(obj);
    }
}