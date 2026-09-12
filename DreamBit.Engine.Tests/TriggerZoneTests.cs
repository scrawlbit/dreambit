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
    public class TriggerZoneTests
    {
        [TestMethod]
        public void Coleta_QuandoOPersonagemSobrepoe()
        {
            var scene = new Scene();

            var player = new GameObject("Player");
            player.AddComponent(new SpriteRenderer { Size = new Vector2(48, 48) });
            player.AddComponent(new PlatformerController { Gravity = 0f, UseKeyboard = false });
            player.Transform.Position = new Vector2(0, 0);
            scene.Add(player);

            var pickup = new GameObject("Moeda");
            pickup.AddComponent(new TriggerZone { Size = new Vector2(32, 32) });
            pickup.Transform.Position = new Vector2(10, 0); // sobrepõe o player
            scene.Add(pickup);

            scene.StartPlay();
            scene.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.016)));

            Assert.IsFalse(pickup.IsVisible, "a moeda deve sumir ao ser coletada");
        }

        [TestMethod]
        public void NaoColeta_QuandoLonge()
        {
            var scene = new Scene();

            var player = new GameObject("Player");
            player.AddComponent(new SpriteRenderer { Size = new Vector2(48, 48) });
            player.AddComponent(new PlatformerController { Gravity = 0f, UseKeyboard = false });
            scene.Add(player);

            var pickup = new GameObject("Moeda");
            pickup.AddComponent(new TriggerZone { Size = new Vector2(32, 32) });
            pickup.Transform.Position = new Vector2(500, 0);
            scene.Add(pickup);

            scene.StartPlay();
            scene.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.016)));

            Assert.IsTrue(pickup.IsVisible);
        }

        private static GameTime Frame => new(TimeSpan.Zero, TimeSpan.FromSeconds(0.016));

        [TestMethod]
        public void FiltraPorTag_SoDisparaComATagAlvo()
        {
            var scene = new Scene();

            // Um inimigo (tag "Enemy") sobrepondo uma zona que só reage a "Player".
            var enemy = new GameObject("Inimigo") { Tag = "Enemy" };
            enemy.AddComponent(new SpriteRenderer { Size = new Vector2(48, 48) });
            enemy.Transform.Position = new Vector2(0, 0);
            scene.Add(enemy);

            var zone = new GameObject("Zona");
            var trigger = new TriggerZone { Size = new Vector2(64, 64), TargetTag = "Player", DestroyOnEnter = false };
            bool entrou = false;
            trigger.Entered += _ => entrou = true;
            zone.AddComponent(trigger);
            zone.Transform.Position = new Vector2(0, 0);
            scene.Add(zone);

            scene.StartPlay();
            scene.Update(Frame);

            Assert.IsFalse(entrou, "inimigo não deve disparar uma zona de tag Player");
        }

        [TestMethod]
        public void DisparaPorTag_MesmoSemPlatformerController()
        {
            var scene = new Scene();

            var hero = new GameObject("Herói") { Tag = "Player" }; // sem PlatformerController
            hero.AddComponent(new SpriteRenderer { Size = new Vector2(40, 40) });
            hero.Transform.Position = new Vector2(0, 0);
            scene.Add(hero);

            var zone = new GameObject("Zona");
            var trigger = new TriggerZone { Size = new Vector2(64, 64), TargetTag = "Player", DestroyOnEnter = false };
            GameObject? quem = null;
            trigger.Entered += obj => quem = obj;
            zone.AddComponent(trigger);
            scene.Add(zone);

            scene.StartPlay();
            scene.Update(Frame);

            Assert.AreSame(hero, quem, "objeto com a tag deve disparar, mesmo sem controller");
        }

        [TestMethod]
        public void EnterEExit_DisparamAoEntrarEAoSair()
        {
            var scene = new Scene();

            var player = new GameObject("Player") { Tag = "Player" };
            player.AddComponent(new SpriteRenderer { Size = new Vector2(20, 20) });
            player.Transform.Position = new Vector2(200, 0); // começa longe
            scene.Add(player);

            var zone = new GameObject("Zona");
            var trigger = new TriggerZone { Size = new Vector2(40, 40), TargetTag = "Player", DestroyOnEnter = false };
            int enters = 0, exits = 0;
            trigger.Entered += _ => enters++;
            trigger.Exited += _ => exits++;
            zone.AddComponent(trigger);
            zone.Transform.Position = new Vector2(0, 0);
            scene.Add(zone);

            scene.StartPlay();

            scene.Update(Frame);                        // longe: nada
            Assert.AreEqual(0, enters);

            player.Transform.Position = new Vector2(0, 0); // entra
            scene.Update(Frame);
            Assert.AreEqual(1, enters);
            Assert.AreEqual(0, exits);

            scene.Update(Frame);                        // continua dentro: não re-dispara enter
            Assert.AreEqual(1, enters);

            player.Transform.Position = new Vector2(200, 0); // sai
            scene.Update(Frame);
            Assert.AreEqual(1, exits);
        }

        [TestMethod]
        public void Serializacao_PreservaTagETrigger()
        {
            var scene = new Scene();
            var obj = new GameObject("Porta") { Tag = "Enemy" };
            obj.AddComponent(new TriggerZone { TargetTag = "Enemy", DestroyOnEnter = false });
            scene.Add(obj);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));

            var restored = loaded.Objects.First();
            Assert.AreEqual("Enemy", restored.Tag);
            var trigger = restored.Components.OfType<TriggerZone>().Single();
            Assert.AreEqual("Enemy", trigger.TargetTag);
            Assert.IsFalse(trigger.DestroyOnEnter);
        }
    }
}
