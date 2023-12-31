﻿using DreamBit.Game.Content;
using DreamBit.Game.Elements;
using DreamBit.Game.Serialization.Converters;
using DreamBit.Project;
using Scrawlbit.Json;

namespace DreamBit.Game.Serialization
{
    public interface IGameElementsParser
    {
        bool DeserializeIds { get; set; }

        string ToJson(GameObject gameObject);
        string ToJson(Scene scene);

        GameObject ToGameObject(string json);
        Scene ToScene(string json);
    }

    internal class GameElementsParser : IGameElementsParser
    {
        private readonly IJsonParser _jsonParser;
        private readonly GameObjectConverter _gameObjectConverter;

        public GameElementsParser(IJsonParser jsonParser, IProject project, IContentManager contentManager)
        {
            _jsonParser = jsonParser;
            _gameObjectConverter = new GameObjectConverter();

            _jsonParser.Converters.Add(_gameObjectConverter);
            _jsonParser.Converters.Add(new SceneConverter());
            _jsonParser.Converters.Add(new Vector2Converter());
            _jsonParser.Converters.Add(new ContentConverter(project, contentManager));
            _jsonParser.Converters.Add(new ColorConverter());
            _jsonParser.Converters.Add(new GameComponentConverter());
            _jsonParser.Converters.Add(new ProjectFileConverter(project));
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
        public string ToJson(Scene scene)
        {
            return _jsonParser.ParseObject(scene);
        }

        public GameObject ToGameObject(string json)
        {
            return _jsonParser.ParseString<GameObject>(json);
        }

        public Scene ToScene(string json)
        {
            return _jsonParser.ParseString<Scene>(json);
        }
    }
}
