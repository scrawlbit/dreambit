using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ScrawlBit.Json.Converters
{
    public abstract class AbstractObjectConveter<T> : JsonConverter
    {
        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var target = CreateInstance(jObject);

            serializer.Populate(jObject.CreateReader(), target);

            return target;
        }
        public override bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }

        protected abstract T CreateInstance(JObject jObject);
    }
}