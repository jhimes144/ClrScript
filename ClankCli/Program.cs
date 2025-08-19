
using Clank;
using System;

string testScript = "var a = 22;\r\n\r\nif (a == 13)\r\n{\r\n    return a;\r\n} else { var t = 12; return t;} return 41;";

Console.WriteLine("Compiling and running test script...");
var context = ClankContext<object, float>.Compile(testScript, null);

var result = context.Run();
Console.WriteLine($"Result: {result}");
