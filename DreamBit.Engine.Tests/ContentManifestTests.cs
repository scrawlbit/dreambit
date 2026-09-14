using DreamBit.Engine.Project;
using Xunit;

namespace DreamBit.Engine.Tests
{
    public class ContentManifestTests
    {
        [Fact]
        public void Generate_IncluiCabecalhoEUmBuildPorAsset()
        {
            var text = ContentManifest.Generate(new[] { "sprites/hero.png", "tiles\\grass.png" }, "DesktopGL");

            StringAssert.Contains(text, "/platform:DesktopGL");
            StringAssert.Contains(text, "/build:sprites/hero.png");
            StringAssert.Contains(text, "/build:tiles/grass.png"); // barras normalizadas
            StringAssert.Contains(text, "/importer:TextureImporter");
        }

        [Fact]
        public void Generate_SemAssets_AindaTemCabecalho()
        {
            var text = ContentManifest.Generate(new string[0]);

            StringAssert.Contains(text, "Global Properties");
            Assert.DoesNotContain("/build:", text);
        }
    }
}
