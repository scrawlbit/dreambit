using DreamBit.Game.Elements;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scrawlbit.Json;
using System;

namespace DreamBit.Game.Serialization.Converters
{
    internal class SceneConverter : JsonConverter<Scene>
    {
        public override Scene ReadJson(JsonReader reader, Type objectType, Scene existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);
            Scene scene = new Scene();

            scene.Objects.Add(obj[nameof(Scene.Objects)].ToObject<GameObject[]>(serializer));

            return scene;
        }
        public override void WriteJson(JsonWriter writer, Scene value, JsonSerializer serializer)
        {
            JObject obj = new JObject();

            obj.SetProperty(nameof(Scene.Objects), value.Objects, serializer);

            obj.WriteTo(writer);
        }
    }
}
