using System;
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
    public class FrameDetectorTests
    {
        // Constrói uma máscara e "pinta" retângulos opacos.
        private static bool[] Mask(int w, int h, params Rectangle[] rects)
        {
            var m = new bool[w * h];
            foreach (var r in rects)
                for (int y = r.Y; y < r.Y + r.Height; y++)
                    for (int x = r.X; x < r.X + r.Width; x++)
                        m[y * w + x] = true;
            return m;
        }

        [TestMethod]
        public void DetectaTresFramesDeTamanhosDiferentes()
        {
            int w = 100, h = 40;
            // três blobs separados por colunas transparentes, alturas/larguras diferentes
            var mask = Mask(w, h,
                new Rectangle(2, 5, 10, 20),   // pequeno
                new Rectangle(30, 2, 20, 30),  // grande
                new Rectangle(70, 10, 15, 15)); // médio

            var frames = FrameDetector.Detect(mask, w, h);

            Assert.AreEqual(3, frames.Count);
            // ordem de leitura (esquerda->direita, já que estão na mesma faixa)
            Assert.AreEqual(new Rectangle(2, 5, 10, 20), frames[0]);
            Assert.AreEqual(new Rectangle(30, 2, 20, 30), frames[1]);
            Assert.AreEqual(new Rectangle(70, 10, 15, 15), frames[2]);
        }

        [TestMethod]
        public void OrdemDeLeituraPorFaixas()
        {
            int w = 60, h = 60;
            // duas linhas de dois frames: ordem esperada TL, TR, BL, BR
            var mask = Mask(w, h,
                new Rectangle(5, 5, 10, 10),    // TL
                new Rectangle(40, 5, 10, 10),   // TR
                new Rectangle(5, 40, 10, 10),   // BL
                new Rectangle(40, 40, 10, 10)); // BR

            var frames = FrameDetector.Detect(mask, w, h);
            Assert.AreEqual(4, frames.Count);
            Assert.AreEqual(new Rectangle(5, 5, 10, 10), frames[0]);
            Assert.AreEqual(new Rectangle(40, 5, 10, 10), frames[1]);
            Assert.AreEqual(new Rectangle(5, 40, 10, 10), frames[2]);
            Assert.AreEqual(new Rectangle(40, 40, 10, 10), frames[3]);
        }

        [TestMethod]
        public void RuidoAbaixoDoMinimoEhIgnorado()
        {
            int w = 30, h = 30;
            var mask = Mask(w, h,
                new Rectangle(2, 2, 10, 10),  // frame
                new Rectangle(25, 25, 1, 1)); // 1 pixel de ruído

            var frames = FrameDetector.Detect(mask, w, h, minPixels: 16);
            Assert.AreEqual(1, frames.Count);
        }

        [TestMethod]
        public void PixelsDiagonaisContamComoUmaRegiao()
        {
            int w = 10, h = 10;
            var mask = Mask(w, h,
                new Rectangle(1, 1, 3, 3),
                new Rectangle(4, 4, 3, 3)); // encosta na diagonal => 8-conexo une
            var frames = FrameDetector.Detect(mask, w, h, minPixels: 1);
            Assert.AreEqual(1, frames.Count);
            Assert.AreEqual(new Rectangle(1, 1, 6, 6), frames[0]);
        }

        [TestMethod]
        public void Animator_UsaFramesExplicitos()
        {
            var anim = new SpriteAnimator { Fps = 10f, Loop = true };
            anim.SetFrames(new[]
            {
                new Rectangle(0, 0, 10, 20),
                new Rectangle(10, 0, 30, 40),
            });
            Assert.AreEqual(2, anim.EffectiveFrameCount);

            anim.Advance(0.1); // 1 frame a 10 fps
            Assert.AreEqual(1, anim.CurrentFrame);
            anim.Advance(0.1); // volta ao 0 (loop, 2 frames)
            Assert.AreEqual(0, anim.CurrentFrame);
        }

        [TestMethod]
        public void Animator_Serializacao_RoundTrip()
        {
            var scene = new Scene();
            var obj = new GameObject("Anim");
            var anim = new SpriteAnimator { TexturePath = "x.png", Fps = 12f };
            anim.SetFrames(new[] { new Rectangle(1, 2, 3, 4), new Rectangle(5, 6, 7, 8) });
            obj.AddComponent(anim);
            scene.Add(obj);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            var a = loaded.Objects.First().Components.OfType<SpriteAnimator>().Single();
            Assert.AreEqual(2, a.Frames.Count);
            Assert.AreEqual(new Rectangle(1, 2, 3, 4), a.Frames[0]);
            Assert.AreEqual(new Rectangle(5, 6, 7, 8), a.Frames[1]);
        }
    }
}
