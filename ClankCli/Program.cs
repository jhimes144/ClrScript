
using Clank;
using System;

string testScript = "var a = 22.12;\r\n\r\nif (a.test && a > 1)\r\n{\r\n aMethod();   return a;\r\n} else { var t = 12; return t;} return 41;";

Console.WriteLine("Compiling and running test script...");
var context = ClankContext<object, double>.Compile(testScript, null);

var result = context.Run();
Console.WriteLine($"Result: {result}");
