using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using DreamBit.Engine.Serialization;
using Xunit;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    public class ChromaKeyTests
    {
        // Imagem 4x4: borda magenta, miolo azul.
        private static Color[] Sample()
        {
            var magenta = new Color(255, 0, 255);
            var azul = new Color(0, 0, 255);
            var px = new Color[16];
            for (int y = 0; y < 4; y++)
                for (int x = 0; x < 4; x++)
                    px[y * 4 + x] = (x == 0 || y == 0 || x == 3 || y == 3) ? magenta : azul;
            return px;
        }

        [Fact]
        public void DetectaCorDeFundoNaBorda()
        {
            var bg = ChromaKey.DetectBackground(Sample(), 4, 4);
            Assert.Equal(new Color(255, 0, 255), bg);
        }

        [Fact]
        public void ApplyTornaTransparenteACorChave()
        {
            var px = Sample();
            int removed = ChromaKey.Apply(px, new Color(255, 0, 255), 0);

            Assert.Equal(12, removed);
            Assert.Equal(0, px[0].A);
            Assert.Equal(255, px[5].A);
        }

        [Fact]
        public void ToleranciaPegaTonsProximos()
        {
            var px = new[] { new Color(250, 5, 250), new Color(0, 0, 255) };
            int removed = ChromaKey.Apply(px, new Color(255, 0, 255), 20); // dif = 5+5+5 = 15 <= 20
            Assert.Equal(1, removed);
            Assert.Equal(0, px[0].A);
            Assert.Equal(255, px[1].A);
        }

        [Fact]
        public void Sprite_Serializacao_RoundTrip()
        {
            var scene = new Scene();
            var obj = new GameObject("S");
            obj.AddComponent(new SpriteRenderer
            {
                TexturePath = "x.png",
                ChromaKeyEnabled = true, ChromaAuto = false,
                ChromaColor = new Color(10, 200, 30), ChromaTolerance = 45
            });
            scene.Add(obj);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            var s = loaded.Objects.First().Components.OfType<SpriteRenderer>().Single();
            Assert.True(s.ChromaKeyEnabled);
            Assert.False(s.ChromaAuto);
            Assert.Equal(new Color(10, 200, 30), s.ChromaColor);
            Assert.Equal(45, s.ChromaTolerance);
        }

        [Fact]
        public void Animator_Serializacao_RoundTrip()
        {
            var scene = new Scene();
            var obj = new GameObject("A");
            obj.AddComponent(new SpriteAnimator { TexturePath = "a.png", ChromaKeyEnabled = true, ChromaTolerance = 12 });
            scene.Add(obj);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            var a = loaded.Objects.First().Components.OfType<SpriteAnimator>().Single();
            Assert.True(a.ChromaKeyEnabled);
            Assert.True(a.ChromaAuto);
            Assert.Equal(12, a.ChromaTolerance);
        }
    }
}
