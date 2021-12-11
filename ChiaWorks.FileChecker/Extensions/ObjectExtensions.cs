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
    }
}