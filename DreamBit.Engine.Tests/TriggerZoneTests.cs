using System;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
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
    }
}
