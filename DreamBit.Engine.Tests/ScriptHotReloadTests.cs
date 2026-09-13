using System;
using System.IO;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class ScriptHotReloadTests
    {
        private static GameTime Frame(float dt) => new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(dt));

        // Script que move o objeto em X por uma velocidade fixa.
        private static string ScriptMovingX(float speed) =>
            "public class Script : IGameScript {\n" +
            "  public void Update(GameObject self, float dt) {\n" +
            $"    self.Transform.Position = new Vector2(self.Transform.Position.X + {speed.ToString(System.Globalization.CultureInfo.InvariantCulture)}f * dt, self.Transform.Position.Y);\n" +
            "  }\n" +
            "}\n";

        [TestMethod]
        public void CarregaDeArquivoExterno()
        {
            var path = Path.Combine(Path.GetTempPath(), "hot_" + Guid.NewGuid().ToString("N") + ".cs");
            try
            {
                File.WriteAllText(path, ScriptMovingX(100f));
                var scene = new Scene();
                var obj = new GameObject("Bot");
                obj.AddComponent(new ScriptComponent { SourcePath = path });
                scene.Add(obj);

                scene.StartPlay();
                scene.Update(Frame(1f)); // move 100
                Assert.AreEqual(100f, obj.Transform.Position.X, 0.5f);
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }

        [TestMethod]
        public void RecarregaQuandoOArquivoMuda()
        {
            var path = Path.Combine(Path.GetTempPath(), "hot_" + Guid.NewGuid().ToString("N") + ".cs");
            try
            {
                File.WriteAllText(path, ScriptMovingX(100f));
                var obj = new GameObject("Bot");
                var script = new ScriptComponent { SourcePath = path };
                obj.AddComponent(script);
                var scene = new Scene();
                scene.Add(obj);
                scene.StartPlay();

                scene.Update(Frame(1f)); // v=100 => x ~100
                float afterFirst = obj.Transform.Position.X;

                // Reescreve o arquivo com nova velocidade e força mtime diferente.
                File.WriteAllText(path, ScriptMovingX(1000f));
                File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddSeconds(5));

                script.ReloadIfChanged();  // hot-reload
                scene.Update(Frame(1f));   // agora v=1000 => +1000
                float afterReload = obj.Transform.Position.X;

                Assert.IsTrue(afterReload - afterFirst > 900f,
                    $"após recarregar deveria mover ~1000 (moveu {afterReload - afterFirst})");
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }

        [TestMethod]
        public void Serializacao_RoundTrip()
        {
            var scene = new Scene();
            var obj = new GameObject("S");
            obj.AddComponent(new ScriptComponent { SourcePath = "scripts/Player.cs" });
            scene.Add(obj);

            var loaded = DreamBit.Engine.Serialization.SceneSerializer.LoadFromString(
                DreamBit.Engine.Serialization.SceneSerializer.SaveToString(scene));
            var s = loaded.Objects.First().Components.OfType<ScriptComponent>().Single();
            Assert.AreEqual("scripts/Player.cs", s.SourcePath);
        }
    }
}
