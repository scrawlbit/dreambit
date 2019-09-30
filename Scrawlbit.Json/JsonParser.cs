using System.Collections.Generic;
using Newtonsoft.Json;

namespace ScrawlBit.Json
{
    public class JsonParser : IJsonParser
    {
        private readonly JsonSerializerSettings _settings;

        public JsonParser()
        {
            _settings = new JsonSerializerSettings();

            ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            Formatting = Formatting.Indented;
        }

        public ReferenceLoopHandling ReferenceLoopHandling
        {
            get => _settings.ReferenceLoopHandling;
            set => _settings.ReferenceLoopHandling = value;
        }
        public Formatting Formatting
        {
            get => _settings.Formatting;
            set => _settings.Formatting = value;
        }
        public IList<JsonConverter> Converters => _settings.Converters;
        
        public string ParseObject(object value)
        {
            return JsonConvert.SerializeObject(value, _settings);
        }
        public T ParseString<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, _settings);
        }
    }
}