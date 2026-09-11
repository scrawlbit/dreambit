using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Serialization
{
    /// <summary>
    /// Serialização de cena em JSON (System.Text.Json). Mantém o modelo (Elements/*)
    /// limpo, com a serialização desacoplada — como faz o DreamBit.Game atual em
    /// Serialization/Converters, aqui reescrito para net8 sem dependência de Newtonsoft.
    /// </summary>
    public static class SceneSerializer
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static void Save(Scene scene, string path)
        {
            var data = ToData(scene);
            File.WriteAllText(path, JsonSerializer.Serialize(data, Options));
        }

        public static Scene Load(string path)
        {
            var data = JsonSerializer.Deserialize<SceneData>(File.ReadAllText(path), Options)
                       ?? new SceneData();
            return FromData(data);
        }

        // ---- modelo -> dados ----

        private static SceneData ToData(Scene scene)
        {
            var data = new SceneData { Name = scene.Name };
            foreach (var obj in scene.Objects)
                data.Objects.Add(ToData(obj));
            return data;
        }

        private static GameObjectData ToData(GameObject obj)
        {
            var t = obj.Transform;
            var data = new GameObjectData
            {
                Id = obj.Id,
                Name = obj.Name,
                IsVisible = obj.IsVisible,
                PositionX = t.Position.X,
                PositionY = t.Position.Y,
                Rotation = t.Rotation,
                ScaleX = t.Scale.X,
                ScaleY = t.Scale.Y
            };

            foreach (var component in obj.Components)
            {
                if (component is SpriteRenderer sprite)
                    data.Sprites.Add(new SpriteData
                    {
                        Width = sprite.Size.X,
                        Height = sprite.Size.Y,
                        R = sprite.Color.R,
                        G = sprite.Color.G,
                        B = sprite.Color.B,
                        A = sprite.Color.A
                    });
                else if (component is RotatorBehavior rotator)
                    data.Rotators.Add(new RotatorData { Speed = rotator.Speed });
            }

            foreach (var child in obj.Children)
                data.Children.Add(ToData(child));

            return data;
        }

        // ---- dados -> modelo ----

        private static Scene FromData(SceneData data)
        {
            var scene = new Scene { Name = data.Name };
            foreach (var objData in data.Objects)
                scene.Add(FromData(objData));
            return scene;
        }

        private static GameObject FromData(GameObjectData data)
        {
            var obj = new GameObject(data.Name) { Id = data.Id, IsVisible = data.IsVisible };
            obj.Transform.Position = new Vector2(data.PositionX, data.PositionY);
            obj.Transform.Rotation = data.Rotation;
            obj.Transform.Scale = new Vector2(data.ScaleX, data.ScaleY);

            foreach (var sprite in data.Sprites)
                obj.AddComponent(new SpriteRenderer
                {
                    Size = new Vector2(sprite.Width, sprite.Height),
                    Color = new Color(sprite.R, sprite.G, sprite.B, sprite.A)
                });

            foreach (var rotator in data.Rotators)
                obj.AddComponent(new RotatorBehavior { Speed = rotator.Speed });

            foreach (var childData in data.Children)
                obj.AddChild(FromData(childData));

            return obj;
        }
    }
}
