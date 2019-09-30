using System.Collections.Generic;
using Newtonsoft.Json;

namespace Scrawlbit.Json
{
    public interface IJsonParser
    {
        ReferenceLoopHandling ReferenceLoopHandling { get; set; }
        Formatting Formatting { get; set; }
        IList<JsonConverter> Converters { get; }
        string ParseObject(object value);
        T ParseString<T>(string json);
    }
}