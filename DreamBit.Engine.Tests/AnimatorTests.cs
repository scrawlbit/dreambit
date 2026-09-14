using DreamBit.Engine.Components;
using Xunit;

namespace DreamBit.Engine.Tests
{
    public class AnimatorTests
    {
        [Fact]
        public void Advance_ComLoop_AvancaEReinicia()
        {
            var a = new SpriteAnimator { Fps = 10f, FrameCount = 4, Loop = true };

            a.Advance(0.1); Assert.Equal(1, a.CurrentFrame);
            a.Advance(0.1); Assert.Equal(2, a.CurrentFrame);
            a.Advance(0.1); Assert.Equal(3, a.CurrentFrame);
            a.Advance(0.1); Assert.Equal(0, a.CurrentFrame); // reinicia
        }

        [Fact]
        public void Advance_SemLoop_ParaNoUltimoFrame()
        {
            var a = new SpriteAnimator { Fps = 10f, FrameCount = 3, Loop = false };

            a.Advance(1.0); // tempo suficiente para passar de todos
            Assert.Equal(2, a.CurrentFrame);
        }

        [Fact]
        public void Advance_FrameUnicoOuFpsZero_NaoAvanca()
        {
            var single = new SpriteAnimator { Fps = 10f, FrameCount = 1 };
            single.Advance(5.0);
            Assert.Equal(0, single.CurrentFrame);

            var stopped = new SpriteAnimator { Fps = 0f, FrameCount = 8 };
            stopped.Advance(5.0);
            Assert.Equal(0, stopped.CurrentFrame);
        }
    }
}
