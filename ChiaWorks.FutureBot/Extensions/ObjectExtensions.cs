using Newtonsoft.Json;

namespace ChiaWorks.FutureBot.Extensions
{
    public static class ObjectExtensions
    {
        public static string Serialize(this object data)
        {
            return JsonConvert.SerializeObject(data);
        }

        public static T Deserialize<T>(this string data) where T : class
        {
            return JsonConvert.DeserializeObject<T>(data);
        }
    }
}