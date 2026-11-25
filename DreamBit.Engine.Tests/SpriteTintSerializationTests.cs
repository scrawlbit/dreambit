using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Microsoft.Xna.Framework;
using Xunit;

namespace DreamBit.Engine.Tests
{
    public class SpriteTintSerializationTests
    {
        [Fact]
        public void TintDoSpriteEhSalvoNaCena()
        {
            var obj = new GameObject("parede");
            obj.AddComponent(new SpriteRenderer { TexturePath = "art/x.png", Tint = new Color(140, 138, 160, 200) });
            var copy = SceneSerializer.LoadPrefabFromString(SceneSerializer.SaveObjectToString(obj));
            var sprite = copy.Components.OfType<SpriteRenderer>().Single();
            Assert.Equal(new Color(140, 138, 160, 200), sprite.Tint);
        }

        [Fact]
        public void CenaAntigaSemTintFicaBranca()
        {
            // Sprites salvos antes do campo existir não têm TintR/G/B/A: o padrão é branco.
            var data = System.Text.Json.JsonSerializer.Deserialize<SpriteData>("{\"Width\":10,\"Height\":10}")!;
            Assert.Equal(new Color(data.TintR, data.TintG, data.TintB, data.TintA), Color.White);
        }
    }
}
