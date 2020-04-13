using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Scrawlbit.Json
{
    public static class JsonHelper
    {
        public static void SetProperty(this JObject obj, string property, object value, JsonSerializer serializer = null)
        {
            if (value != null)
                obj[property] = JToken.FromObject(value, serializer);
        }
        public static void SetProperty(this JObject obj, JsonProperty property, object from, JsonSerializer serializer = null)
        {
            object value = property.ValueProvider.GetValue(from);

            obj.SetProperty(property.PropertyName, value, serializer);
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
