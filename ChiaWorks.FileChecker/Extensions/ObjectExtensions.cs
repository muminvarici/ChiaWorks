using System;
using Newtonsoft.Json;

namespace ChiaWorks.FileChecker.Extensions
{
    public static class ObjectExtensions
    {
        private static readonly JsonSerializerSettings JsonFormatterSettings =
            new()
            {
                Formatting = Formatting.Indented
            };

        public static string ToJson(this object obj) => JsonConvert.SerializeObject(obj, settings: JsonFormatterSettings);

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