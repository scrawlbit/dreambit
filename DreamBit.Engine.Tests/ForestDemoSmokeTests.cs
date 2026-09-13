using System;
using System.IO;
using System.Linq;
using DreamBit.Engine.Audio;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using DreamBit.Engine.Serialization;
using DreamBit.Engine.Tests.Demo;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using GameInput = DreamBit.Engine.Input.Input;

namespace DreamBit.Engine.Tests
{
    /// <summary>
    /// Smoke test integrado da fase "Floresta": monta a cena com os assets reais e exercita, de
    /// ponta a ponta e sem tela, os sistemas da engine (clipes de sprite, plataforma+tilemap,
    /// pathfinding, física rígida, timeline, trigger/mensagem, áudio espacial, serialização).
    /// Ao final grava o .dbscene jogável em DemoAssets/forest-demo.dbscene.
    /// </summary>
    [TestClass]
    public class ForestDemoSmokeTests
    {
        private static GameTime Frame(double s = 0.016) => new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(s));

        private static void Step(Scene scene, params string[] hold)
        {
            GameInput.Update();
            foreach (var a in hold)
                GameInput.HoldAction(a);
            scene.Update(Frame());
        }

        private static bool AssetsPresent()
            => File.Exists(Path.Combine(ForestDemo.AssetsDir, "hero-frames.json"));

        private static GameObject Find(Scene scene, string name)
            => scene.VisibleInDrawOrder().First(o => o.Name == name);

        [TestMethod]
        public void Demo_ExercitaTodosOsSistemas_EGravaCena()
        {
            if (!AssetsPresent())
                Assert.Inconclusive("DemoAssets/hero-frames.json ausente — rode a detecção de frames primeiro.");

            Screen.Set(1280, 720);
            AudioMixer.Reset();
            GameInput.SetPointer(new Vector2(-1, -1), false);

            var scene = ForestDemo.Build();
            var hero = Find(scene, "Heroi");
            var anim = hero.Components.OfType<SpriteAnimator>().Single();
            var enemy = Find(scene, "Inimigo");

            // Clipes recortados da sheet real.
            Assert.AreEqual(156, anim.Frames.Count, "156 frames detectados na sheet do herói");
            Assert.AreEqual(5, anim.Clips.Count, "clipes idle/walk/jump/attack/death");

            string? message = null;
            scene.MessageSent += m => message = m.Name;
            scene.StartPlay();

            // 1) Assenta no chão do tilemap -> idle.
            for (int i = 0; i < 45; i++) Step(scene);
            var plat = hero.Components.OfType<PlatformerController>().Single();
            Assert.IsTrue(plat.Grounded, "herói pousa no tilemap sólido");
            Assert.AreEqual("idle", anim.CurrentClip);
            float restY = hero.Transform.Position.Y;

            // 2) Anda para a direita -> walk, sem flip, avança.
            float x0 = hero.Transform.Position.X;
            for (int i = 0; i < 20; i++) Step(scene, "MoveRight");
            Assert.AreEqual("walk", anim.CurrentClip);
            Assert.IsFalse(anim.FlipX);
            Assert.IsTrue(hero.Transform.Position.X > x0 + 20f, "herói avançou andando");

            // 3) Anda para a esquerda -> vira (flip).
            for (int i = 0; i < 6; i++) Step(scene, "MoveLeft");
            Assert.IsTrue(anim.FlipX, "vira ao andar para a esquerda");

            // 4) Pula -> jump, sai do chão.
            for (int i = 0; i < 4; i++) Step(scene, "Jump");
            Assert.AreEqual("jump", anim.CurrentClip);
            Assert.IsFalse(plat.Grounded, "no ar durante o pulo");

            // deixa pousar de novo (ar ~0.8s)
            for (int i = 0; i < 70; i++) Step(scene);
            Assert.IsTrue(plat.Grounded);

            // 5) Ataca -> attack tem prioridade, toca uma vez.
            Step(scene, "Action");
            Assert.AreEqual("attack", anim.CurrentClip);

            // 6) Inimigo persegue (pathfinding/linha reta) e se aproxima do herói.
            float enemyDist0 = Vector2.Distance(enemy.Transform.Position, hero.Transform.Position);
            for (int i = 0; i < 60; i++) Step(scene);
            float enemyDist1 = Vector2.Distance(enemy.Transform.Position, hero.Transform.Position);
            Assert.IsTrue(enemyDist1 < enemyDist0 - 20f, $"inimigo se aproximou ({enemyDist0:0}->{enemyDist1:0})");

            // 6b) Combate: encosta o inimigo e ataca — o evento do golpe ativa a hitbox e fere.
            var enemyHp = enemy.Components.OfType<Health>().Single();
            float enemyHpBefore = enemyHp.Current;
            GameInput.Update(); GameInput.HoldAction("Action"); scene.Update(Frame(0.016));
            for (int i = 0; i < 40; i++)
            {
                enemy.Transform.Position = hero.Transform.Position + new Vector2(90, 0); // mantém encostado
                Step(scene);
            }
            Assert.IsTrue(enemyHp.Current < enemyHpBefore,
                $"o golpe do herói feriu o inimigo ({enemyHpBefore:0}->{enemyHp.Current:0})");

            // 7) Física rígida: as caixas caem e assentam.
            var topCrate = Find(scene, "Caixa2");
            Assert.IsTrue(topCrate.Transform.Position.Y > 260f, "caixa dinâmica caiu sob gravidade");

            // 8) Timeline de propriedade animou o cristal (posição saiu do valor inicial).
            var crystal = Find(scene, "Cristal");
            Assert.AreNotEqual(420f, crystal.Transform.Position.Y, "PropertyAnimator moveu o cristal");

            // 9) Trigger de meta: leva o herói até a tenda -> mensagem "reach_tent".
            hero.Transform.Position = new Vector2(2300, 470);
            for (int i = 0; i < 5; i++) Step(scene);
            Assert.AreEqual("reach_tent", message, "chegar à tenda dispara a mensagem de meta");

            GameInput.ClearPointerOverride();

            // 10) Serialização round-trip preserva os sistemas, e grava o .dbscene jogável.
            var json = SceneSerializer.SaveToString(ForestDemo.Build());
            var reload = SceneSerializer.LoadFromString(json);
            var rHero = reload.VisibleInDrawOrder().First(o => o.Name == "Heroi");
            Assert.AreEqual(5, rHero.Components.OfType<SpriteAnimator>().Single().Clips.Count);
            Assert.IsTrue(reload.VisibleInDrawOrder().Any(o => o.Components.OfType<NavChaser>().Any()));
            Assert.IsTrue(reload.VisibleInDrawOrder().Any(o => o.Components.OfType<TilemapRenderer>().Any(t => t.Solid)));

            File.WriteAllText(Path.Combine(ForestDemo.AssetsDir, "forest-demo.dbscene"), json);
        }
    }
}
