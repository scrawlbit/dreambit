using DreamBit.Game.Elements;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scrawlbit.Json;
using System;
using System.Linq;
using GameComponent = DreamBit.Game.Elements.GameComponent;

namespace DreamBit.Game.Serialization.Converters
{
    internal class GameObjectConverter : JsonConverter<GameObject>
    {
        public bool DeserializeIds { get; set; } = true;

        public override GameObject ReadJson(JsonReader reader, Type objectType, GameObject existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);
            GameObject instance = new GameObject();

            if (DeserializeIds)
                instance.Id = obj[nameof(GameObject.Id)].ToObject<Guid>(serializer);

            instance.Name = obj[nameof(GameObject.Name)].ToString();
            instance.IsVisible = obj[nameof(GameObject.IsVisible)].ToObject<bool>(serializer);
            instance.Transform.Relative.Position = obj[nameof(Transform.Position)].ToObject<Vector2>(serializer);
            instance.Transform.Relative.Rotation = obj[nameof(Transform.Rotation)].ToObject<float>(serializer);
            instance.Transform.Relative.Scale = obj[nameof(Transform.Scale)].ToObject<Vector2>(serializer);

            if (obj.TryGetPropertyValue(nameof(GameObject.Components), serializer, out GameComponent[] components))
                instance.Components.Add(components);

            if (obj.TryGetPropertyValue(nameof(GameObject.Children), serializer, out GameObject[] children))
                instance.Children.Add(children);

            return instance;
        }
        public override void WriteJson(JsonWriter writer, GameObject value, JsonSerializer serializer)
        {
            JObject obj = new JObject();

            obj.SetProperty(nameof(GameObject.Id), value.Id, serializer);
            obj.SetProperty(nameof(GameObject.Name), value.Name, serializer);
            obj.SetProperty(nameof(GameObject.IsVisible), value.IsVisible, serializer);
            obj.SetProperty(nameof(Transform.Position), value.Transform.Relative.Position, serializer);
            obj.SetProperty(nameof(Transform.Rotation), value.Transform.Relative.Rotation, serializer);
            obj.SetProperty(nameof(Transform.Scale), value.Transform.Relative.Scale, serializer);

            if (value.Components.Any())
                obj.SetProperty(nameof(GameObject.Components), value.Components, serializer);

            if (value.Children.Any())
                obj.SetProperty(nameof(GameObject.Children), value.Children, serializer);

            obj.WriteTo(writer);
        }
    }
}
