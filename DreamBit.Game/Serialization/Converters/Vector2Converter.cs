using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scrawlbit.Helpers;
using System;

namespace DreamBit.Game.Serialization.Converters
{
    internal class Vector2Converter : JsonConverter<Vector2>
    {
        public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            string[] parts = token.ToString().Split(';');
            float x = parts[0].ToFloat();
            float y = parts[1].ToFloat();

            return new Vector2(x, y);
        }
        public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
        {
            string s = $"{value.X};{value.Y}";
            JToken token = JToken.FromObject(s);

            token.WriteTo(writer);
        }
    }
}
