using System.IO;
using DreamBit.Game.Reading.Converters;
using Scrawlbit.Json;
using Newtonsoft.Json.Converters;
using Scrawlbit.Injection;

namespace DreamBit.Game.Reading
{
    internal class DataReader : IDataReader
    {
        private readonly IContainer _container;
        private IJsonParser _jsonParser;

        public DataReader(IContainer container)
        {
            _container = container;
        }

        public T Load<T>(string assetName, string extension)
        {
            if (_jsonParser == null)
            {
                _jsonParser = _container.Resolve<IJsonParser>();
                _jsonParser.Converters.Add(new StringEnumConverter());
                _jsonParser.Converters.Add(_container.Resolve<GameObjectConverter>());
                _jsonParser.Converters.Add(_container.Resolve<GameObjectComponentConverter>());
            }

            var json = File.ReadAllText($"Content/{assetName}.{extension}");
            var obj = _jsonParser.ParseString<T>(json);

            return obj;
        }
    }
}