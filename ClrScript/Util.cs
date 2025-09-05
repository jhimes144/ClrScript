using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript
{
    public static class Util
    {
        public static T[] CreateInstancesOfInterface<T>() where T : class
        {
            var interfaceType = typeof(T);
            var assembly = Assembly.GetExecutingAssembly();

            var implementingTypes = assembly.GetTypes()
                .Where(type => interfaceType.IsAssignableFrom(type) &&
                       !type.IsInterface &&
                       !type.IsAbstract &&
                       type.GetConstructor(Type.EmptyTypes) != null)
                .ToArray();

            var instances = implementingTypes
                .Select(type => (T)Activator.CreateInstance(type))
                .ToArray();

            return instances;
        }

        public static string ConvertStrToCamel(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var words = input.Split(new char[] { ' ', '_', '-', '.' }, StringSplitOptions.RemoveEmptyEntries);

            if (words.Length == 0)
                return string.Empty;

            if (words.Length == 1)
            {
                var word = words[0];
                if (word.Length == 0) return string.Empty;
                if (word.Length == 1) return word.ToLower();

                return char.ToLower(word[0]) + word.Substring(1);
            }

            var result = new StringBuilder();

            result.Append(words[0].ToLower());

            for (int i = 1; i < words.Length; i++)
            {
                var word = words[i];
                if (!string.IsNullOrEmpty(word))
                {
                    result.Append(char.ToUpper(word[0]));
                    if (word.Length > 1)
                    {
                        result.Append(word.Substring(1).ToLower());
                    }
                }
            }

            return result.ToString();
        }
    }
}
