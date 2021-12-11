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
    }
}