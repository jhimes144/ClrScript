
using Clank;
using System;
using System.Runtime.CompilerServices;

// string testScript = "var a = {\r\n name: \"jake\",\r\n ageObj: age\r\n};\r\n\r\nvar p = {\r\n  age: 12\r\n};\r\n\r\na.ageObj.age = 34;\r\n\r\nreturn a.ageObj.age;";

var testScript = "for (var i = 1; i <= 50000; i = i + 1)\r\n{\r\n\tprint \"hello\";\r\n}";

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