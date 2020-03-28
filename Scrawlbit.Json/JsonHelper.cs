using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Scrawlbit.Json
{
    public static class JsonHelper
    {
        public static JToken SetProperty(this JObject obj, string property, object value, JsonSerializer serializer = null)
        {
            return obj[property] = JToken.FromObject(value, serializer);
        }

        public static bool TryGetPropertyValue<T>(this JObject obj, string property, out T value)
        {
            return obj.TryGetPropertyValue(property, null, out value);
        }
        public static bool TryGetPropertyValue<T>(this JObject obj, string property, JsonSerializer serializer, out T value)
        {
            JToken token = obj[property];

            if (token != null && token.HasValues)
            {
                value = token.ToObject<T>(serializer);
                return true;
            }

            value = default;
            return false;
        }
    }
}
