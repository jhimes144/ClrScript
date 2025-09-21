using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Tests
{
    [TestClass]
    public class Comments
    {
        [TestMethod]
        public void Single_Line_Comment_Basic()
        {
            var code = @"
                // This is a comment
                var x = 5;
                return x;
            ";

            var context = ClrScriptContext<object>.Compile(code);
            var result = context.Run();
            Assert.AreEqual(5.0, result);
        }

        [TestMethod]
        public void Single_Line_Comment_End_Of_Line()
        {
            var code = @"
                var x = 10; // This is an end-of-line comment
                var y = 20; // Another comment
                return x + y;
            ";

            var context = ClrScriptContext<object>.Compile(code);
            var result = context.Run();
            Assert.AreEqual(30.0, result);
        }

        [TestMethod]
        public void Multiple_Single_Line_Comments()
        {
            var code = @"
                // First comment
                // Second comment
                // Third comment
                var result = 42;
                // Comment before return
                return result;
            ";

            var context = ClrScriptContext<object>.Compile(code);
            var result = context.Run();
            Assert.AreEqual(42.0, result);
        }

        [TestMethod]
        public void Comments_In_Function_Body()
        {
            var code = @"
                var calculate = (x, y) -> {
                    // Calculate the sum
                    var sum = x + y;
                    // Calculate the product
                    var product = x * y;
                    // Return the sum of both
                    return sum + product;
                };
                
                return calculate(12, 12);
            ";

            var context = ClrScriptContext<object>.Compile(code);
            var result = context.Run();
            Assert.AreEqual(168d, result);
        }

        [TestMethod]
        public void Comments_In_Control_Structures()
        {
            var code = @"
                var x = 10;
                
                // Check if x is greater than 5
                if (x > 5) {
                    // x is greater than 5
                    x = x * 2;
                } else {
                    // x is not greater than 5
                    x = x + 1;
                }
                
                // Return the result
                return x;
            ";

            var context = ClrScriptContext<object>.Compile(code);
            var result = context.Run();
            Assert.AreEqual(20.0, result);
        }

        [TestMethod]
        public void Comments_In_Loops()
        {
            var code = @"
                var sum = 0;
                
                // Loop from 1 to 5
                for (var i = 1; i <= 5; i++) {
                    // Add i to sum
                    sum = sum + i;
                }
                
                // Return the total sum
                return sum;
            ";

            var context = ClrScriptContext<object>.Compile(code);
            var result = context.Run();
            Assert.AreEqual(15.0, result); // 1 + 2 + 3 + 4 + 5 = 15
        }

        [TestMethod]
        public void Comments_With_Object_Literals()
        {
            var code = @"
                // Create a person object
                var person = {
                    name: ""John"", // Person's name
                    age: 30        // Person's age
                };
                
                // Update the age
                person.age = 31;
                
                // Return the updated age
                return person.age;
            ";

            var context = ClrScriptContext<object>.Compile(code);
            var result = context.Run();
            Assert.AreEqual(31.0, result);
        }

        [TestMethod]
        public void Comments_With_String_Literals()
        {
            var code = @"
                // Test that comments don't interfere with strings containing //
                var message = ""This string contains // but it's not a comment"";
                var url = ""https://example.com/path"";
                
                // Return the length of both strings combined
                return message.length() + url.length();
            ";

            var context = ClrScriptContext<object>.Compile(code);
            var result = context.Run();
            Assert.AreEqual(70d, result); // Length of both strings
            Assert.IsFalse(context.DynamicOperationsEmitted);
        }

        [TestMethod]
        public void Empty_Comments()
        {
            var code = @"
                //
                var x = 5;
                //
                var y = 10;
                //
                return x + y;
            ";

            var context = ClrScriptContext<object>.Compile(code);
            var result = context.Run();
            Assert.AreEqual(15.0, result);
        }

        [TestMethod]
        public void Comments_With_Special_Characters()
        {
            var code = @"
                // Comment with special chars: !@#$%^&*()_+-={}[]|;':"",./<>?
                var result = 100;
                // Another comment with unicode: αβγδε
                return result;
            ";

            var context = ClrScriptContext<object>.Compile(code);
            var result = context.Run();
            Assert.AreEqual(100.0, result);
        }

        [TestMethod]
        public void Comments_At_End_Of_File()
        {
            var code = @"
                var x = 25;
                return x;
                // This comment is at the end of the file";

            var context = ClrScriptContext<object>.Compile(code);
            var result = context.Run();
            Assert.AreEqual(25.0, result);
        }

        [TestMethod]
        public void Nested_Comments_In_Blocks()
        {
            var code = @"
                var result = 0;
                
                {
                    // Comment in nested block
                    var temp = 5;
                    {
                        // Deeper nested comment
                        temp = temp * 2;
                    }
                    result = temp;
                }
                
                return result;
            ";

            var context = ClrScriptContext<object>.Compile(code);
            var result = context.Run();
            Assert.AreEqual(10.0, result);
        }

        [TestMethod]
        public void Comments_With_While_Loop()
        {
            var code = @"
                var count = 0;
                var sum = 0;
                
                // While loop with comments
                while (count < 3) {
                    count++; // Increment counter
                    sum = sum + count; // Add to sum
                }
                
                // Return final sum
                return sum;
            ";

            var context = ClrScriptContext<object>.Compile(code);
            var result = context.Run();
            Assert.AreEqual(6.0, result); // 1 + 2 + 3 = 6
        }
    }
}
