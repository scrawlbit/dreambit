using System;
using System.IO;
using DreamBit.Engine.Assets;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class SceneAssetResolverTests
    {
        private string _root = "";

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "dbscene_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(_root, "art"));
        }

        [TestCleanup]
        public void Cleanup() { try { Directory.Delete(_root, true); } catch { } }

        [TestMethod]
        public void CenaResolveAssetMovido_PeloGuid()
        {
            var asset = Path.Combine(_root, "art", "hero.png");
            File.WriteAllText(asset, "x");
            var db = new AssetDatabase(_root);

            // Cena referencia o asset por caminho relativo.
            var scene = new Scene();
            var o = new GameObject("Hero");
            o.AddComponent(new SpriteRenderer { TexturePath = "art/hero.png" });
            scene.Add(o);

            var scenePath = Path.Combine(_root, "fase.dbscene");
            SceneSerializer.SaveWithAssets(scene, scenePath, db, _root);

            // Move/renomeia o asset (e o .meta).
            var newDir = Path.Combine(_root, "sprites");
            Directory.CreateDirectory(newDir);
            var newAsset = Path.Combine(newDir, "personagem.png");
            File.Move(asset, newAsset);
            File.Move(asset + ".meta", newAsset + ".meta");
            db.Refresh();

            // Ao carregar, o caminho quebrado é reescrito para a nova localização.
            var loaded = SceneSerializer.LoadWithAssets(scenePath, db, _root);
            var sprite = loaded.Objects.First().Components.OfType<SpriteRenderer>().Single();
            Assert.AreEqual("sprites/personagem.png", sprite.TexturePath);
        }

        [TestMethod]
        public void CaminhoValido_NaoEAlterado()
        {
            var asset = Path.Combine(_root, "art", "ok.png");
            File.WriteAllText(asset, "x");
            var db = new AssetDatabase(_root);
            var scene = new Scene();
            var o = new GameObject("O");
            o.AddComponent(new SpriteRenderer { TexturePath = "art/ok.png" });
            scene.Add(o);

            var scenePath = Path.Combine(_root, "f.dbscene");
            SceneSerializer.SaveWithAssets(scene, scenePath, db, _root);
            var loaded = SceneSerializer.LoadWithAssets(scenePath, db, _root);
            Assert.AreEqual("art/ok.png", loaded.Objects.First().Components.OfType<SpriteRenderer>().Single().TexturePath);
        }
    }
}
