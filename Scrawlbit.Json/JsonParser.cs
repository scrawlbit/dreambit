using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Scrawlbit.Json
{
    public interface IJsonParser
    {
        IContractResolver ContractResolver { get; set; }
        ReferenceLoopHandling ReferenceLoopHandling { get; set; }
        Formatting Formatting { get; set; }
        IList<JsonConverter> Converters { get; }
        string ParseObject(object value);
        T ParseString<T>(string json);
    }

    public class JsonParser : IJsonParser
    {
        private readonly JsonSerializerSettings _settings;

        public JsonParser()
        {
            _settings = new JsonSerializerSettings();
            _settings.Culture = CultureInfo.InvariantCulture;

            ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            Formatting = Formatting.Indented;
        }

        public IContractResolver ContractResolver
        {
            get => _settings.ContractResolver;
            set => _settings.ContractResolver = value;
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