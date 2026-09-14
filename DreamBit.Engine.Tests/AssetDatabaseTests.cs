using System;
using System.IO;
using DreamBit.Engine.Assets;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class AssetDatabaseTests
    {
        private string _dir = "";

        [TestInitialize]
        public void Setup()
        {
            _dir = Path.Combine(Path.GetTempPath(), "dbassets_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dir);
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { Directory.Delete(_dir, true); } catch { }
        }

        [TestMethod]
        public void GuidForFile_Estavel_ECriaMeta()
        {
            var asset = Path.Combine(_dir, "hero.png");
            File.WriteAllText(asset, "x");
            var db = new AssetDatabase(_dir);

            var g1 = db.GuidForFile(asset);
            Assert.IsTrue(File.Exists(asset + ".meta"), "cria o sidecar .meta");
            var g2 = db.GuidForFile(asset);
            Assert.AreEqual(g1, g2, "o GUID é estável entre chamadas");
        }

        [TestMethod]
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
            Assert.AreEqual(newAsset, resolved, "resolve pelo GUID mesmo após mover/renomear");
        }
    }
}
