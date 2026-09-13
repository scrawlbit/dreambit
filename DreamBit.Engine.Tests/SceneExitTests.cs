using System;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class SceneExitTests
    {
        private static GameTime Frame => new(TimeSpan.Zero, TimeSpan.FromSeconds(0.016));

        [TestMethod]
        public void Player_NaSaida_SolicitaProximaFase()
        {
            var scene = new Scene();

            var player = new GameObject("Heroi") { Tag = "Player" };
            player.AddComponent(new SpriteRenderer { Size = new Vector2(40, 40) });
            player.Transform.Position = new Vector2(500, 0); // longe da saída
            scene.Add(player);

            var exit = new GameObject("Saida");
            exit.AddComponent(new SceneExit { Size = new Vector2(60, 200), TargetTag = "Player", TargetScene = "level2.dbscene" });
            exit.Transform.Position = new Vector2(0, 0);
            scene.Add(exit);

            scene.StartPlay();
            scene.Update(Frame);
            Assert.IsNull(scene.PendingSceneLoad, "longe: nada");

            player.Transform.Position = new Vector2(0, 0); // entra na saída
            scene.Update(Frame);
            Assert.AreEqual("level2.dbscene", scene.PendingSceneLoad, "na saída: pede a próxima fase");
        }

        [TestMethod]
        public void SceneExit_RoundTrip()
        {
            var scene = new Scene();
            var obj = new GameObject("S");
            obj.AddComponent(new SceneExit { Size = new Vector2(70, 220), TargetTag = "Player", TargetScene = "game.dbscene" });
            scene.Add(obj);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            var exit = loaded.Objects.First().Components.OfType<SceneExit>().Single();
            Assert.AreEqual("game.dbscene", exit.TargetScene);
            Assert.AreEqual(new Vector2(70, 220), exit.Size);
        }
    }
}
