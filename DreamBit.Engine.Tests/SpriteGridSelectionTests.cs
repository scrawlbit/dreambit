using DreamBit.Engine.Components;
using Microsoft.Xna.Framework;
using Xunit;

namespace DreamBit.Engine.Tests
{
    /// <summary>
    /// Seleção de animação por grade direto na folha original (sem recortar os quadros):
    /// cada índice global mapeia para uma célula inteira, preservando tamanho e transparência.
    /// </summary>
    public class SpriteGridSelectionTests
    {
        private static SpriteAnimator Grid() =>
            new SpriteAnimator { FrameWidth = 300, FrameHeight = 300, FrameCount = 156 }; // 13x12

        [Fact]
        public void Grade_SelecionaCelulaInteira_SemRecortar()
        {
            var anim = Grid();
            const int cols = 13;

            Assert.Empty(anim.Frames); // nada recortado: usa a grade
            Assert.Equal(new Rectangle(0, 0, 300, 300), anim.FrameRect(0, cols));       // linha 0, col 0
            Assert.Equal(new Rectangle(0, 300, 300, 300), anim.FrameRect(13, cols));    // linha 1, col 0
            Assert.Equal(new Rectangle(3600, 3000, 300, 300), anim.FrameRect(142, cols)); // col 12, linha 10

            // Toda célula tem o tamanho cheio (300x300), com a transparência preservada.
            for (int i = 0; i < 156; i++)
            {
                var r = anim.FrameRect(i, cols);
                Assert.Equal(300, r.Width);
                Assert.Equal(300, r.Height);
            }
        }

        [Fact]
        public void Clip_MapeiaIndicesGlobais_NasCelulasDaGrade()
        {
            var anim = Grid();
            var idle = SpriteClip.Range("idle", 65, 13);

            Assert.Equal(13, idle.Frames.Length);
            Assert.Equal(new Rectangle(0, 1500, 300, 300), anim.FrameRect(idle.Frames[0], 13));    // 65 -> col 0, linha 5
            Assert.Equal(new Rectangle(3600, 1500, 300, 300), anim.FrameRect(idle.Frames[12], 13)); // 77 -> col 12, linha 5
        }

        [Fact]
        public void FramesExplicitos_TemPrioridade_SobreAGrade()
        {
            var anim = Grid();
            anim.SetFrames(new[] { new Rectangle(7, 9, 50, 60) });
            Assert.Equal(new Rectangle(7, 9, 50, 60), anim.FrameRect(0, 13)); // usa o recorte, ignora a grade
        }
    }
}
