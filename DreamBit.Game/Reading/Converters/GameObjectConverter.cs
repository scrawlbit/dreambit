using System;
using DreamBit.Game.Elements;
using DreamBit.Game.Elements.Components;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scrawlbit.Helpers;

namespace DreamBit.Game.Reading.Converters
{
    internal class GameObjectConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var gameObject = new GameObject();
            var children = jObject["Children"].ToObject<GameObject[]>(serializer);
            var components = jObject["Components"].ToObject<GameObjectComponent[]>(serializer);
            var position = jObject["Position"].ToObject<Vector2>();
            var rotation = jObject["Rotation"].ToObject<float>();
            var scale = jObject["Scale"].ToObject<Vector2>();
            
            serializer.Populate(jObject.CreateReader(), gameObject);

            gameObject.Transform.RelativePosition = position;
            gameObject.Transform.RelativeRotation = rotation;
            gameObject.Transform.RelativeScale = scale;
            children.ForEach(gameObject.AddChild);
            components.ForEach(gameObject.AddComponent);

            return gameObject;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IGameObject);
        }
    }
}