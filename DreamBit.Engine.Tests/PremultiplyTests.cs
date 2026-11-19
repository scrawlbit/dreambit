using DreamBit.Engine.Rendering;
using Microsoft.Xna.Framework;
using Xunit;

namespace DreamBit.Engine.Tests
{
    public class PremultiplyTests
    {
        [Fact]
        public void PixelTransparenteComCor_ViraPreto()
        {
            var px = new[] { new Color(255, 200, 100, 0), new Color(200, 100, 50, 128), new Color(10, 20, 30, 255) };
            TextureCache.Premultiply(px);
            Assert.Equal(new Color(0, 0, 0, 0), px[0]);
            Assert.Equal(new Color(100, 50, 25, 128), px[1]);
            Assert.Equal(new Color(10, 20, 30, 255), px[2]);
        }
    }
}
