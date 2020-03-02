using System.IO;
using System.Text;
using Scrawlbit.Json;

namespace DreamBit.Game.Writing
{
    internal class DataWriter : IDataWriter
    {
        private readonly IJsonParser _jsonParser;
        
        public DataWriter(IJsonParser jsonParser)
        {
            _jsonParser = jsonParser;
        }

        public void Save<T>(T data, string assetName, string extension, Encoding encoding)
        {
            File.WriteAllText($"Content/{assetName}.{extension}", _jsonParser.ParseObject(data), encoding);
        }
    }
}