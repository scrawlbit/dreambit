using DreamBit.Engine.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xunit;

namespace DreamBit.Engine.Tests
{
    public class MirrorDecomposeTests
    {
        // Mapeia um ponto local do quad como o SpriteBatch faria (rotação + escala + flip vertical).
        private static Vector2 AsSpriteBatch(Matrix world, Vector2 local)
        {
            var (pos, rot, sx, sy, mirror) = SceneRenderer.Decompose(world);
            float y = mirror.HasFlag(SpriteEffects.FlipVertically) ? -local.Y : local.Y;
            var scaled = new Vector2(local.X * sx, y * sy);
            return pos + Vector2.Transform(scaled, Matrix.CreateRotationZ(rot));
        }

        [Theory]
        [InlineData(1f, 0.3f)]
        [InlineData(-1f, 0.3f)]
        [InlineData(-1f, 2.2f)]
        [InlineData(-1.5f, -1f)] // escala uniforme: escala não uniforme + rotação vira cisalhamento, que o SpriteBatch não representa
        public void DecomposeReproduzAMatriz_InclusiveEspelhada(float parentScaleX, float childRotation)
        {
            var parent = Matrix.CreateScale(parentScaleX, System.Math.Abs(parentScaleX), 1f) * Matrix.CreateTranslation(50, 20, 0);
            var child = Matrix.CreateRotationZ(childRotation) * Matrix.CreateTranslation(10, -30, 0);
            var world = child * parent;

            foreach (var p in new[] { new Vector2(0, -10), new Vector2(7, 4), new Vector2(-3, 12) })
            {
                var expected = Vector2.Transform(p, world);
                var actual = AsSpriteBatch(world, p);
                Assert.Equal(expected.X, actual.X, 3);
                Assert.Equal(expected.Y, actual.Y, 3);
            }
        }

        [Fact]
        public void SoEspelhaComDeterminanteNegativo()
        {
            Assert.Equal(SpriteEffects.None, SceneRenderer.Decompose(Matrix.CreateRotationZ(1f)).Mirror);
            Assert.Equal(SpriteEffects.FlipVertically, SceneRenderer.Decompose(Matrix.CreateScale(-1f, 1f, 1f)).Mirror);
        }
    }
}
