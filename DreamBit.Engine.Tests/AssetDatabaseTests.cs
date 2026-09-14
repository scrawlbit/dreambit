using System;
using System.IO;
using DreamBit.Engine.Assets;
using Xunit;

namespace DreamBit.Engine.Tests
{
    public class AssetDatabaseTests : IDisposable
    {
        private string _dir = "";

        public AssetDatabaseTests()
        {
            _dir = Path.Combine(Path.GetTempPath(), "dbassets_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dir);
        }

        public void Dispose()
        {
            try { Directory.Delete(_dir, true); } catch { }
        }

        [Fact]
        public void GuidForFile_Estavel_ECriaMeta()
        {
            var asset = Path.Combine(_dir, "hero.png");
            File.WriteAllText(asset, "x");
            var db = new AssetDatabase(_dir);

            var g1 = db.GuidForFile(asset);
            Assert.True(File.Exists(asset + ".meta"), "cria o sidecar .meta");
            var g2 = db.GuidForFile(asset);
            Assert.Equal(g1, g2);
        }

        [Fact]
        public void PathForGuid_SobreviveARenomear()
        {
            var asset = Path.Combine(_dir, "sprite.png");
            File.WriteAllText(asset, "x");
            var db = new AssetDatabase(_dir);
            var guid = db.GuidForFile(asset);

            // Renomeia o asset e o seu .meta (como um refactor de projeto).
            var renamed = Path.Combine(_dir, "sub");
            Directory.CreateDirectory(renamed);
            var newAsset = Path.Combine(renamed, "personagem.png");
            File.Move(asset, newAsset);
            File.Move(asset + ".meta", newAsset + ".meta");

            var resolved = db.PathForGuid(guid);
            Assert.Equal(newAsset, resolved);
        }
    }
}
