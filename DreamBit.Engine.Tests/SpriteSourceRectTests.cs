using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Xunit;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    public class SpriteSourceRectTests
    {
        [Fact]
        public void SourceRect_LarguraOuAlturaZero_ViraNull()
        {
            var sprite = new SpriteRenderer { SourceRect = new Rectangle(0, 0, 0, 10) };
            Assert.Null(sprite.SourceRect);

            sprite.SourceRect = new Rectangle(4, 8, 16, 24);
            Assert.Equal(new Rectangle(4, 8, 16, 24), sprite.SourceRect);
        }

        [Fact]
        public void Serializacao_PreservaSourceRect()
        {
            var scene = new Scene();
            var obj = new GameObject("Prop");
            obj.AddComponent(new SpriteRenderer
            {
                TexturePath = "atlas.png",
                SourceRect = new Rectangle(100, 40, 64, 48),
                Size = new Vector2(64, 48)
            });
            scene.Add(obj);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));

            var sprite = loaded.Objects.First().Components.OfType<SpriteRenderer>().Single();
            Assert.Equal(new Rectangle(100, 40, 64, 48), sprite.SourceRect);
        }

        [Fact]
        public void Serializacao_SemRecorte_ContinuaNull()
        {
            var scene = new Scene();
            var obj = new GameObject("Cheio");
            obj.AddComponent(new SpriteRenderer { TexturePath = "img.png" });
            scene.Add(obj);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));

            var sprite = loaded.Objects.First().Components.OfType<SpriteRenderer>().Single();
            Assert.Null(sprite.SourceRect);
        }
    }
}
