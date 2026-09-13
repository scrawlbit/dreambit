using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class SpriteSourceRectTests
    {
        [TestMethod]
        public void SourceRect_LarguraOuAlturaZero_ViraNull()
        {
            var sprite = new SpriteRenderer { SourceRect = new Rectangle(0, 0, 0, 10) };
            Assert.IsNull(sprite.SourceRect, "recorte sem área deve ser tratado como null");

            sprite.SourceRect = new Rectangle(4, 8, 16, 24);
            Assert.AreEqual(new Rectangle(4, 8, 16, 24), sprite.SourceRect);
        }

        [TestMethod]
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
            Assert.AreEqual(new Rectangle(100, 40, 64, 48), sprite.SourceRect);
        }

        [TestMethod]
        public void Serializacao_SemRecorte_ContinuaNull()
        {
            var scene = new Scene();
            var obj = new GameObject("Cheio");
            obj.AddComponent(new SpriteRenderer { TexturePath = "img.png" });
            scene.Add(obj);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));

            var sprite = loaded.Objects.First().Components.OfType<SpriteRenderer>().Single();
            Assert.IsNull(sprite.SourceRect);
        }
    }
}
