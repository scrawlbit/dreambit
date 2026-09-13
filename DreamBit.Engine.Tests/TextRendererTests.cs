using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using DreamBit.Engine.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class TextRendererTests
    {
        [TestMethod]
        public void PixelFont_MedeLargura()
        {
            Assert.AreEqual(0, PixelFont.MeasureWidth(""));
            Assert.AreEqual(3, PixelFont.MeasureWidth("A"));      // 3 largura
            Assert.AreEqual(7, PixelFont.MeasureWidth("AB"));     // 3 + 1 + 3
            Assert.AreEqual(5, PixelFont.Glyph('A').Length);       // 5 linhas
        }

        [TestMethod]
        public void PixelFont_DesconhecidoViraEspaco()
        {
            CollectionAssert.AreEqual(PixelFont.Glyph(' '), PixelFont.Glyph('❤'));
        }

        [TestMethod]
        public void Serializacao_RoundTrip()
        {
            var scene = new Scene();
            var obj = new GameObject("HUD");
            obj.AddComponent(new TextRenderer { Text = "SCORE: 100", Color = new Color(200, 50, 30), PixelSize = 6, ScreenSpace = true });
            scene.Add(obj);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            var t = loaded.Objects.First().Components.OfType<TextRenderer>().Single();
            Assert.AreEqual("SCORE: 100", t.Text);
            Assert.AreEqual(6, t.PixelSize);
            Assert.IsTrue(t.ScreenSpace);
            Assert.AreEqual((byte)200, t.Color.R);
        }
    }
}
