using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using DreamBit.Engine.Serialization;
using Xunit;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    public class TextRendererTests
    {
        [Fact]
        public void PixelFont_MedeLargura()
        {
            Assert.Equal(0, PixelFont.MeasureWidth(""));
            Assert.Equal(3, PixelFont.MeasureWidth("A"));      // 3 largura
            Assert.Equal(7, PixelFont.MeasureWidth("AB"));     // 3 + 1 + 3
            Assert.Equal(5, PixelFont.Glyph('A').Length);       // 5 linhas
        }

        [Fact]
        public void PixelFont_DesconhecidoViraEspaco()
        {
            CollectionAssert.AreEqual(PixelFont.Glyph(' '), PixelFont.Glyph('❤'));
        }

        [Fact]
        public void Serializacao_RoundTrip()
        {
            var scene = new Scene();
            var obj = new GameObject("HUD");
            obj.AddComponent(new TextRenderer { Text = "SCORE: 100", Color = new Color(200, 50, 30), PixelSize = 6, ScreenSpace = true });
            scene.Add(obj);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            var t = loaded.Objects.First().Components.OfType<TextRenderer>().Single();
            Assert.Equal("SCORE: 100", t.Text);
            Assert.Equal(6, t.PixelSize);
            Assert.True(t.ScreenSpace);
            Assert.Equal((byte)200, t.Color.R);
        }
    }
}
