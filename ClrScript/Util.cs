using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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

        public static T TryGetAtIndex<T>(this IReadOnlyList<T> list, int index)
        {
            if (index >= 0 && list.Count < index)
            {
                return list[index];
            }

            return default;
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

        public static bool IsExtensionMethod(MethodInfo method)
        {
            return method.IsStatic &&
                   method.IsDefined(typeof(ExtensionAttribute), false) &&
                   method.GetParameters().Length > 0;
        }

        public static Type CreateDelegateType(Type returnType, params Type[] types)
        {
            var allTypes = new List<Type>(types);
            allTypes.Add(returnType);

            switch (allTypes.Count)
            {
                case 1:
                    return typeof(Func<>).MakeGenericType(returnType);
                case 2:
                    return typeof(Func<,>).MakeGenericType(allTypes.ToArray());
                case 3:
                    return typeof(Func<,,>).MakeGenericType(allTypes.ToArray());
                case 4:
                    return typeof(Func<,,,>).MakeGenericType(allTypes.ToArray());
                case 5:
                    return typeof(Func<,,,,>).MakeGenericType(allTypes.ToArray());
                case 6:
                    return typeof(Func<,,,,,>).MakeGenericType(allTypes.ToArray());
                case 7:
                    return typeof(Func<,,,,,,>).MakeGenericType(allTypes.ToArray());
                case 8:
                    return typeof(Func<,,,,,,,>).MakeGenericType(allTypes.ToArray());
                case 9:
                    return typeof(Func<,,,,,,,,>).MakeGenericType(allTypes.ToArray());
                case 10:
                    return typeof(Func<,,,,,,,,,>).MakeGenericType(allTypes.ToArray());
                case 11:
                    return typeof(Func<,,,,,,,,,,>).MakeGenericType(allTypes.ToArray());
                case 12:
                    return typeof(Func<,,,,,,,,,,,>).MakeGenericType(allTypes.ToArray());
                case 13:
                    return typeof(Func<,,,,,,,,,,,,>).MakeGenericType(allTypes.ToArray());
                case 14:
                    return typeof(Func<,,,,,,,,,,,,,>).MakeGenericType(allTypes.ToArray());
                case 15:
                    return typeof(Func<,,,,,,,,,,,,,,>).MakeGenericType(allTypes.ToArray());
                case 16:
                    return typeof(Func<,,,,,,,,,,,,,,,>).MakeGenericType(allTypes.ToArray());
                case 17:
                    return typeof(Func<,,,,,,,,,,,,,,,,>).MakeGenericType(allTypes.ToArray());
                default:
                    throw new ArgumentException($"Too many parameters ({types.Length}). Maximum supported is 16 parameters plus return type.");
            }
        }
    }
}
