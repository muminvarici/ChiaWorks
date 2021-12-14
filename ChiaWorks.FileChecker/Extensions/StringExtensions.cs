using System;

namespace ChiaWorks.FileChecker.Extensions
{
    public static class StringExtensions
    {
        public static string ReverseString(this string str)
        {
            if (str == null)
                return null;
            var charArray = str.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public static bool IsNullOrWhiteSpace(this string str) => str != null && string.IsNullOrWhiteSpace(str);
        public static string Format(this string str, params object[] args) => string.Format(str, args);
    }
}