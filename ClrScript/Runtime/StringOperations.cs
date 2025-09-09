using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Runtime
{
    public static class StringOperations
    {
        [ClrScriptMember(ConvertToCamelCase = true)]
        public static double Length(string str)
        {
            return str.Length;
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static string PadLeft(string str, double amount)
        {
            return str.PadLeft((int)amount);
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static string PadRight(string str, double amount)
        {
            return str.PadRight((int)amount);
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static string PadLeft(string str, double amount, string paddingChar)
        {
            if (string.IsNullOrEmpty(paddingChar))
            {
                return str.PadLeft((int)amount);
            }
            return str.PadLeft((int)amount, paddingChar[0]);
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static string PadRight(string str, double amount, string paddingChar)
        {
            if (string.IsNullOrEmpty(paddingChar))
            {
                return str.PadRight((int)amount);
            }
            return str.PadRight((int)amount, paddingChar[0]);
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static string Substring(string str, double startIndex)
        {
            if (startIndex < 0 || startIndex >= str.Length)
            {
                return "";
            }
            return str.Substring((int)startIndex);
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static string Substring(string str, double startIndex, double length)
        {
            if (startIndex < 0 || startIndex >= str.Length)
            {
                return "";
            }
            int start = (int)startIndex;
            int len = (int)length;
            if (start + len > str.Length)
            {
                len = str.Length - start;
            }
            if (len <= 0)
            {
                return "";
            }
            return str.Substring(start, len);
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static string ToUpper(string str)
        {
            return str.ToUpper();
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static string ToLower(string str)
        {
            return str.ToLower();
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static string Trim(string str)
        {
            return str.Trim();
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static string TrimStart(string str)
        {
            return str.TrimStart();
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static string TrimEnd(string str)
        {
            return str.TrimEnd();
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static double IndexOf(string str, string searchValue)
        {
            return str.IndexOf(searchValue);
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static double IndexOf(string str, string searchValue, double startIndex)
        {
            if (startIndex < 0 || startIndex >= str.Length)
            {
                return -1;
            }
            return str.IndexOf(searchValue, (int)startIndex);
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static double LastIndexOf(string str, string searchValue)
        {
            return str.LastIndexOf(searchValue);
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static bool Contains(string str, string searchValue)
        {
            return str.Contains(searchValue);
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static bool StartsWith(string str, string prefix)
        {
            return str.StartsWith(prefix);
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static bool EndsWith(string str, string suffix)
        {
            return str.EndsWith(suffix);
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static string Replace(string str, string oldValue, string newValue)
        {
            return str.Replace(oldValue, newValue);
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static string Insert(string str, double startIndex, string value)
        {
            if (startIndex < 0 || startIndex > str.Length)
            {
                return str;
            }
            return str.Insert((int)startIndex, value);
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static string Remove(string str, double startIndex)
        {
            if (startIndex < 0 || startIndex >= str.Length)
            {
                return str;
            }
            return str.Remove((int)startIndex);
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static string Remove(string str, double startIndex, double count)
        {
            if (startIndex < 0 || startIndex >= str.Length)
            {
                return str;
            }
            int start = (int)startIndex;
            int cnt = (int)count;
            if (start + cnt > str.Length)
            {
                cnt = str.Length - start;
            }
            if (cnt <= 0)
            {
                return str;
            }
            return str.Remove(start, cnt);
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static string Concat(string str1, string str2)
        {
            return string.Concat(str1, str2);
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static string Concat(string str1, string str2, string str3)
        {
            return string.Concat(str1, str2, str3);
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static string Concat(string str1, string str2, string str3, string str4)
        {
            return string.Concat(str1, str2, str3, str4);
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static bool IsEmpty(string str)
        {
            return string.IsNullOrEmpty(str);
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static bool IsEmptyOrWhiteSpace(string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static string Repeat(string str, double count)
        {
            if (count <= 0)
            {
                return "";
            }
            var sb = new StringBuilder();
            for (int i = 0; i < (int)count; i++)
            {
                sb.Append(str);
            }
            return sb.ToString();
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static string Reverse(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            var chars = str.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static string CharAt(string str, double index)
        {
            if (index < 0 || index >= str.Length)
            {
                return string.Empty;
            }

            return str[(int)index].ToString();
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static double Compare(string str1, string str2)
        {
            return string.Compare(str1, str2);
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static double CompareIgnoreCase(string str1, string str2)
        {
            return string.Compare(str1, str2, StringComparison.OrdinalIgnoreCase);
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static bool Equals(string str1, string str2)
        {
            return string.Equals(str1, str2);
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static bool EqualsIgnoreCase(string str1, string str2)
        {
            return string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static string Join(string separator, string str1, string str2)
        {
            return string.Join(separator, str1, str2);
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static string Join(string separator, string str1, string str2, string str3)
        {
            return string.Join(separator, str1, str2, str3);
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public static string Join(string separator, string str1, string str2, string str3, string str4)
        {
            return string.Join(separator, str1, str2, str3, str4);
        }
    }
}
