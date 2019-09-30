using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Scrawlbit.Json.Converters
{
    public class DependencyInstanceConverter<T> : JsonConverter
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly object _instance;
        
        public DependencyInstanceConverter(T instance)
        {
            _instance = instance;
        }
        public DependencyInstanceConverter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var target = _instance ?? _serviceProvider.GetService(typeof(T));

            serializer.Populate(jsonObject.CreateReader(), target);

            return target;
        }
        public override bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }
    }
}