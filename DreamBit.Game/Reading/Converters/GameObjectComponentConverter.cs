using System;
using DreamBit.Game.Content;
using DreamBit.Game.Elements.Components;
using DreamBit.Game.Factory;
using DreamBit.Game.Reading.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scrawlbit.Helpers;

namespace DreamBit.Game.Reading.Converters
{
    internal class GameObjectComponentConverter : JsonConverter
    {
        private readonly IGameObjectComponentFactory _componentFactory;
        private readonly IContentReferenceManager _contentReferenceManager;

        public GameObjectComponentConverter(IGameObjectComponentFactory componentFactory, IContentReferenceManager contentReferenceManager)
        {
            _componentFactory = componentFactory;
            _contentReferenceManager = contentReferenceManager;
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var component = CreateInstance(jObject);
            var properties = component.GetType().GetProperties();

            jObject.Property("Type").Remove();

            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(Guid))
                    continue;

                var propertyName = property.Attribute<ContentReferenceAttribute>()?.PropertyName ?? property.Name;
                var value = jObject.Property(propertyName)?.Value.ToString();

                if (Guid.TryParse(value, out Guid fileId))
                    _contentReferenceManager.Prepare(component, property, fileId);
            }

            serializer.Populate(jObject.CreateReader(), component);

            return component;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(GameObjectComponent).IsAssignableFrom(objectType);
        }

        private GameObjectComponent CreateInstance(JObject jObject)
        {
            var type = jObject["Type"].ToString();
            var fileId = jObject["FileId"]?.ToString();
            var componentType = EnumHelper.Parse<ComponentType>(type);

            switch (componentType)
            {
                case ComponentType.ImageRenderer: return _componentFactory.CreateImageRenderer();
                case ComponentType.TextRenderer: return _componentFactory.CreateTextRenderer();
                case ComponentType.Camera: return _componentFactory.CreateCameraObject();
                case ComponentType.ScriptBehavior: return _componentFactory.CreateScriptBehavior(fileId);
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}