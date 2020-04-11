using DreamBit.Game.Elements;
using DreamBit.Game.Elements.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Scrawlbit.Json;
using System;

namespace DreamBit.Game.Serialization.Converters
{
    internal class GameObjectComponentConverter : JsonConverter<GameObjectComponent>
    {
        private const string Type = "Type";
        private const string Image = "Image";

        public override GameObjectComponent ReadJson(JsonReader reader, Type objectType, GameObjectComponent existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            string typeName = jObject[Type].ToString();
            GameObjectComponent component = CreateInstance(typeName);

            serializer.Populate(jObject.CreateReader(), component);

            return component;
        }
        public override void WriteJson(JsonWriter writer, GameObjectComponent value, JsonSerializer serializer)
        {
            JsonObjectContract contract = (JsonObjectContract)serializer.ContractResolver.ResolveContract(value.GetType());
            JObject obj = new JObject();
            string typeName = GetTypeName(value);

            if (typeName != null)
                obj[Type] = typeName;

            foreach (var property in contract.Properties)
            {
                if (property.Readable && property.Writable)
                    obj.SetProperty(property, value, serializer);
            }

            obj.WriteTo(writer);
        }

        private static string GetTypeName(GameObjectComponent value)
        {
            switch (value)
            {
                case ImageRenderer _: return Image;
                default: return null;
            }
        }
        private static GameObjectComponent CreateInstance(string typeName)
        {
            switch (typeName)
            {
                case Image: return new ImageRenderer();
                default: throw new NotImplementedException();
            }
        }
    }
}
