using System.Collections.Generic;
using System.Linq;
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

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> items) => items == null || !items.Any();
        public static string ToJson(this object obj) => JsonConvert.SerializeObject(obj, settings: JsonFormatterSettings);
    }
}