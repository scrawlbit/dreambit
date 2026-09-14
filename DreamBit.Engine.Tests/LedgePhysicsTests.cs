using DreamBit.Engine.Elements;
using DreamBit.Engine.Physics;
using Xunit;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    public class LedgePhysicsTests
    {
        private static Ledge Flat()
        {
            var l = new Ledge();
            l.SetPoints(new[] { new Vector2(0, 0), new Vector2(100, 0) });
            return l;
        }

        [Fact]
        public void SurfaceYAt_Plano_RetornaAAltura()
        {
            var l = Flat();
            Assert.Equal(0f, LedgePhysics.SurfaceYAt(l, 50), 0.01f);
            Assert.True(float.IsNaN(LedgePhysics.SurfaceYAt(l, -10)));
            Assert.True(float.IsNaN(LedgePhysics.SurfaceYAt(l, 200)));
        }

        [Fact]
        public void SurfaceYAt_Rampa_Interpola()
        {
            var l = new Ledge();
            l.SetPoints(new[] { new Vector2(0, 0), new Vector2(100, 100) });

            Assert.Equal(50f, LedgePhysics.SurfaceYAt(l, 50), 0.01f);
            Assert.Equal(25f, LedgePhysics.SurfaceYAt(l, 25), 0.01f);
        }

        [Fact]
        public void FindLanding_PousaAoCruzarDeCima()
        {
            var ledges = new[] { Flat() };

            // pés cruzando y=0 ao cair (de -5 para 5)
            Assert.Equal(0f, LedgePhysics.FindLanding(ledges, 50, -5, 5), 0.01f);
            // ainda acima (de -20 para -10): não pousa
            Assert.True(float.IsNaN(LedgePhysics.FindLanding(ledges, 50, -20, -10)));
            // já abaixo (de 10 para 20): não pousa
            Assert.True(float.IsNaN(LedgePhysics.FindLanding(ledges, 50, 10, 20)));
            // fora do span em X: não pousa
            Assert.True(float.IsNaN(LedgePhysics.FindLanding(ledges, 200, -5, 5)));
        }
    }
}
