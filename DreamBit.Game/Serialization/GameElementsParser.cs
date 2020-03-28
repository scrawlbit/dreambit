﻿using DreamBit.Game.Elements;
using DreamBit.Game.Serialization.Converters;
using Scrawlbit.Json;

namespace DreamBit.Game.Serialization
{
    public interface IGameElementsParser
    {
        bool DeserializeIds { get; set; }

        string ToJson(GameObject gameObject);
        GameObject ToGameObject(string json);
    }

    internal class GameElementsParser : IGameElementsParser
    {
        private readonly IJsonParser _jsonParser;
        private readonly GameObjectConverter _gameObjectConverter;

        public GameElementsParser(IJsonParser jsonParser)
        {
            _jsonParser = jsonParser;
            _gameObjectConverter = new GameObjectConverter();

            _jsonParser.Converters.Add(new Vector2Converter());
            _jsonParser.Converters.Add(_gameObjectConverter);
        }

        public bool DeserializeIds
        {
            get => _gameObjectConverter.DeserializeIds;
            set => _gameObjectConverter.DeserializeIds = value;
        }

        public string ToJson(GameObject gameObject)
        {
            return _jsonParser.ParseObject(gameObject);
        }
        public GameObject ToGameObject(string json)
        {
            return _jsonParser.ParseString<GameObject>(json);
        }
    }
}
