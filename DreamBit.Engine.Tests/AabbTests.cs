using DreamBit.Engine.Geometry;
using Microsoft.Xna.Framework;
using Xunit;

namespace DreamBit.Engine.Tests
{
    public class AabbTests
    {
        [Fact]
        public void FromCenterSize_CalculaMinMax()
        {
            var b = Aabb.FromCenterSize(new Vector2(10, 20), new Vector2(40, 60));
            Assert.Equal(new Vector2(-10, -10), b.Min);
            Assert.Equal(new Vector2(30, 50), b.Max);
            Assert.Equal(new Vector2(10, 20), b.Center);
        }

        [Fact]
        public void Overlaps_EstritoVsInclusivo()
        {
            var a = Aabb.FromCenterSize(Vector2.Zero, new Vector2(100, 100));   // [-50,50]
            var dentro = Aabb.FromCenterSize(new Vector2(40, 0), new Vector2(100, 100));
            var encostando = Aabb.FromCenterSize(new Vector2(100, 0), new Vector2(100, 100)); // borda em 50
            var fora = Aabb.FromCenterSize(new Vector2(200, 0), new Vector2(100, 100));

            Assert.True(a.Overlaps(dentro));
            Assert.False(a.Overlaps(encostando));   // estrito: encostar não conta
            Assert.True(a.Intersects(encostando));  // inclusivo: encostar conta
            Assert.False(a.Intersects(fora));
        }

        [Fact]
        public void Contains_Ponto()
        {
            var a = Aabb.FromCenterSize(Vector2.Zero, new Vector2(20, 20)); // [-10,10]
            Assert.True(a.Contains(new Vector2(5, -5)));
            Assert.False(a.Contains(new Vector2(11, 0)));
        }
    }
}
