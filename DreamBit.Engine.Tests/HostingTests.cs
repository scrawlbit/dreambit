using System.IO;
using DreamBit.Engine.Assets;
using DreamBit.Engine.Hosting;
using Microsoft.Xna.Framework;
using Xunit;

namespace DreamBit.Engine.Tests
{
    public class HostingTests
    {
        [Fact]
        public void Letterbox_PreservaProporcao_ECentraliza()
        {
            var r = SceneHost.Letterbox(new Point(1920, 1080), 1280, 1024);
            Assert.Equal(1280, r.Width);
            Assert.Equal(720, r.Height);
            Assert.Equal(0, r.X);
            Assert.Equal((1024 - 720) / 2, r.Y);
        }

        [Fact]
        public void Letterbox_JanelaMaisLarga_BarrasLaterais()
        {
            var r = SceneHost.Letterbox(new Point(1920, 1080), 2560, 1080);
            Assert.Equal(1920, r.Width);
            Assert.Equal(1080, r.Height);
            Assert.Equal(320, r.X);
        }

        [Fact]
        public void AssetPaths_ProcuraNasRaizesRegistradas()
        {
            var dir = Path.Combine(Path.GetTempPath(), "dbassets_" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(dir, "img"));
            var file = Path.Combine(dir, "img", "a.png");
            File.WriteAllText(file, "x");
            try
            {
                AssetPaths.AddRoot(dir);
                Assert.Equal(Path.GetFullPath(file), Path.GetFullPath(AssetPaths.Resolve("img/a.png")));
                var absolute = Path.Combine(Path.GetTempPath(), "nao-existe.png");
                Assert.Equal(absolute, AssetPaths.Resolve(absolute));
            }
            finally
            {
                AssetPaths.ClearRoots();
                Directory.Delete(dir, true);
            }
        }

        [Fact]
        public void SetScene_IniciaPlayEDisparaEvento()
        {
            using var host = new SceneHost();
            Engine.Elements.Scene? loaded = null;
            host.SceneLoaded += s => loaded = s;
            var scene = new Engine.Elements.Scene { Name = "x" };
            host.SetScene(scene);
            Assert.Same(scene, host.Scene);
            Assert.Same(scene, loaded);
        }
    }
}
