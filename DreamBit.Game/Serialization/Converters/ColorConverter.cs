using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scrawlbit.Util.Helpers;
using System;

namespace DreamBit.Game.Serialization.Converters
{
    internal class ColorConverter : JsonConverter<Color>
    {
        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            string[] parts = token.ToString().Split(';');
            byte r = parts[0].ToByte();
            byte g = parts[1].ToByte();
            byte b = parts[2].ToByte();
            byte a = parts[3].ToByte();

            return new Color(r, g, b, a);
        }
        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
        {
            string s = $"{value.R};{value.G};{value.B};{value.A}";
            JToken token = JToken.FromObject(s, serializer);

            token.WriteTo(writer);
        }
    }
}
