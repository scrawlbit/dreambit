using DreamBit.Game.Elements;
using DreamBit.Game.Elements.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Scrawlbit.Json;
using System;

namespace DreamBit.Game.Serialization.Converters
{
    internal class GameComponentConverter : JsonConverter<GameComponent>
    {
        private const string Type = "Type";
        private const string Image = "Image";

        public override GameComponent ReadJson(JsonReader reader, Type objectType, GameComponent existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            string typeName = jObject[Type].ToString();
            GameComponent component = CreateInstance(typeName);

            serializer.Populate(jObject.CreateReader(), component);

            return component;
        }
        public override void WriteJson(JsonWriter writer, GameComponent value, JsonSerializer serializer)
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

        private static string GetTypeName(GameComponent value)
        {
            switch (value)
            {
                case ImageRenderer _: return Image;
                default: return null;
            }
        }
        private static GameComponent CreateInstance(string typeName)
        {
            switch (typeName)
            {
                case Image: return new ImageRenderer();
                default: throw new NotImplementedException();
            }
        }
    }
}
