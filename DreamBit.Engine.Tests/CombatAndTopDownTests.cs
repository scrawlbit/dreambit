using System;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using GameInput = DreamBit.Engine.Input.Input;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class CombatAndTopDownTests
    {
        private static GameTime Frame(double s = 0.016) => new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(s));

        // ---------- Vida ----------

        [TestMethod]
        public void Health_Dano_Invuln_Morte_Destroy()
        {
            var scene = new Scene();
            var o = new GameObject("Alvo");
            var hp = new Health { Max = 100, InvulnTime = 0.2f, DestroyOnDeath = true, SendOnDeath = "morreu" };
            o.AddComponent(hp);
            scene.Add(o);
            string? msg = null; scene.MessageSent += m => msg = m.Name;
            scene.StartPlay();

            hp.Damage(30);
            Assert.AreEqual(70f, hp.Current, 0.001f);
            hp.Damage(30); // dentro da invulnerabilidade -> ignorado
            Assert.AreEqual(70f, hp.Current, 0.001f, "i-frames bloqueiam dano seguido");

            for (int i = 0; i < 20; i++) scene.Update(Frame()); // passa a invuln
            hp.Damage(80);
            Assert.IsTrue(hp.IsDead);
            scene.Update(Frame()); // processa mensagem + destroy adiado
            Assert.AreEqual("morreu", msg);
            Assert.IsFalse(scene.Objects.Contains(o), "DestroyOnDeath remove o objeto");
        }

        // ---------- Hitbox x Hurtbox ----------

        private static (Scene, Health) BuildDuel(int attackerTeam, int targetTeam, out Hitbox hitbox)
        {
            var scene = new Scene();
            var attacker = new GameObject("Atacante");
            attacker.Transform.Position = new Vector2(0, 0);
            hitbox = new Hitbox { Team = attackerTeam, Damage = 25, Width = 60, Height = 60, ActiveTime = 0.15f };
            attacker.AddComponent(hitbox);
            scene.Add(attacker);

            var target = new GameObject("Alvo");
            target.Transform.Position = new Vector2(30, 0);
            var hp = new Health { Max = 100, InvulnTime = 0f };
            target.AddComponent(hp);
            target.AddComponent(new Hurtbox { Team = targetTeam, Width = 48, Height = 48 });
            scene.Add(target);

            scene.StartPlay();
            GameInput.SetPointer(new Vector2(-1, -1), false);
            return (scene, hp);
        }

        [TestMethod]
        public void Hitbox_FereTimeDiferente_UmaVezPorAtivacao()
        {
            var (scene, hp) = BuildDuel(0, 1, out var hitbox);
            hitbox.Activate();
            scene.Update(Frame());
            Assert.AreEqual(75f, hp.Current, 0.001f, "acerta uma vez");
            scene.Update(Frame());
            Assert.AreEqual(75f, hp.Current, 0.001f, "não acerta de novo na mesma ativação");

            hitbox.Activate(); // nova ativação (invuln do alvo = 0)
            scene.Update(Frame());
            Assert.AreEqual(50f, hp.Current, 0.001f, "nova ativação acerta de novo");
            GameInput.ClearPointerOverride();
        }

        [TestMethod]
        public void Hitbox_NaoFereOMesmoTime()
        {
            var (scene, hp) = BuildDuel(1, 1, out var hitbox);
            hitbox.Activate();
            scene.Update(Frame());
            Assert.AreEqual(100f, hp.Current, 0.001f, "mesmo time não recebe dano");
            GameInput.ClearPointerOverride();
        }

        [TestMethod]
        public void Hitbox_AtivaPorMensagem()
        {
            var scene = new Scene();
            var attacker = new GameObject("A");
            var hitbox = new Hitbox { Team = 0, Damage = 40, ActivateOn = "golpe", Width = 60, Height = 60 };
            attacker.AddComponent(hitbox);
            scene.Add(attacker);
            var target = new GameObject("B");
            target.Transform.Position = new Vector2(20, 0);
            var hp = new Health { InvulnTime = 0f };
            target.AddComponent(hp);
            target.AddComponent(new Hurtbox { Team = 1, Width = 48, Height = 48 });
            scene.Add(target);
            scene.StartPlay();

            scene.Send("golpe");
            scene.Update(Frame()); // frame 1: despacha a mensagem no fim (ativa a hitbox)
            scene.Update(Frame()); // frame 2: hitbox ativa varre e acerta
            Assert.AreEqual(60f, hp.Current, 0.001f, "mensagem ativou a hitbox");
        }

        // ---------- SpriteFlash ----------

        [TestMethod]
        public void SpriteFlash_PiscaAoLevarDano()
        {
            var scene = new Scene();
            var o = new GameObject("Herói");
            var anim = new SpriteAnimator { FrameCount = 4 };
            o.AddComponent(anim);
            var hp = new Health { InvulnTime = 0f };
            o.AddComponent(hp);
            o.AddComponent(new SpriteFlash { FlashColor = Color.Red, Duration = 0.1f });
            scene.Add(o);
            scene.StartPlay();

            scene.Update(Frame());
            Assert.AreEqual(Color.White, anim.Tint, "sem dano: sem flash");

            hp.Damage(10);
            scene.Update(Frame());
            Assert.AreEqual(Color.Red, anim.Tint, "pisca ao levar dano");

            for (int i = 0; i < 20; i++) scene.Update(Frame());
            Assert.AreEqual(Color.White, anim.Tint, "volta ao normal após o flash");
        }

        // ---------- TopDownController ----------

        [TestMethod]
        public void TopDown_MoveEColideComSolido()
        {
            var scene = new Scene();
            var wall = new GameObject("Parede");
            wall.Transform.Position = new Vector2(250, 100);
            wall.AddComponent(new BoxCollider { Size = new Vector2(40, 400) });
            scene.Add(wall);

            var o = new GameObject("Jogador");
            o.Transform.Position = new Vector2(100, 100);
            var td = new TopDownController { MoveSpeed = 200f, HalfWidth = 10f, HalfHeight = 10f };
            o.AddComponent(td);
            scene.Add(o);
            scene.StartPlay();
            GameInput.SetPointer(new Vector2(-1, -1), false);

            for (int i = 0; i < 120; i++) { GameInput.Update(); GameInput.HoldAction("MoveRight"); scene.Update(Frame()); }
            Assert.IsTrue(o.Transform.Position.X > 120f, "andou para a direita");
            Assert.IsTrue(o.Transform.Position.X <= 231f, "parou na parede sólida (x=" + o.Transform.Position.X + ")");
            GameInput.ClearPointerOverride();
        }

        [TestMethod]
        public void TopDown_ComAnimator_MoveVerticalUsaWalkNaoJump()
        {
            var scene = new Scene();
            var o = new GameObject("Jogador");
            var anim = new SpriteAnimator { FrameCount = 12 };
            anim.AddClip(SpriteClip.Range("idle", 0, 1));
            anim.AddClip(SpriteClip.Range("walk", 1, 4));
            anim.AddClip(SpriteClip.Range("jump", 5, 2, 10f, false));
            o.AddComponent(anim);
            o.AddComponent(new TopDownController { MoveSpeed = 150f });
            o.AddComponent(new SpriteAnimatorController());
            scene.Add(o);
            scene.StartPlay();
            GameInput.SetPointer(new Vector2(-1, -1), false);

            for (int i = 0; i < 4; i++) { GameInput.Update(); GameInput.HoldAction("MoveDown"); scene.Update(Frame()); }
            Assert.AreEqual("walk", anim.CurrentClip, "movimento vertical top-down usa walk, não jump");
            GameInput.ClearPointerOverride();
        }

        // ---------- Serialização ----------

        [TestMethod]
        public void Serializacao_RoundTrip_CombateETopDown()
        {
            var scene = new Scene();
            var o = new GameObject("E");
            o.AddComponent(new TopDownController { MoveSpeed = 175f, HalfWidth = 12f });
            o.AddComponent(new Health { Max = 60, DestroyOnDeath = true, SendOnDeath = "x" });
            o.AddComponent(new Hurtbox { Team = 2, Width = 40, Offset = new Vector2(0, 8) });
            o.AddComponent(new Hitbox { Team = 2, Damage = 15, ActivateOn = "atk" });
            o.AddComponent(new SpriteFlash { FlashColor = Color.Red });
            scene.Add(o);

            var e = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene)).Objects.First();
            Assert.AreEqual(175f, e.Components.OfType<TopDownController>().Single().MoveSpeed);
            Assert.AreEqual(60f, e.Components.OfType<Health>().Single().Max);
            Assert.IsTrue(e.Components.OfType<Health>().Single().DestroyOnDeath);
            Assert.AreEqual(2, e.Components.OfType<Hurtbox>().Single().Team);
            Assert.AreEqual("atk", e.Components.OfType<Hitbox>().Single().ActivateOn);
            Assert.AreEqual(Color.Red, e.Components.OfType<SpriteFlash>().Single().FlashColor);
        }
    }
}
