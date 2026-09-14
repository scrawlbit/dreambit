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
    public class SpriteClipTests
    {
        private static GameTime Frame(double s) => new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(s));

        private static SpriteAnimator GridAnimator(int frameCount)
            => new SpriteAnimator { FrameWidth = 32, FrameHeight = 32, FrameCount = frameCount, Fps = 8f };

        [TestMethod]
        public void Clipe_ComLoop_RepeteSoOsFramesDoClipe()
        {
            var a = GridAnimator(10);
            a.AddClip(new SpriteClip("walk", new[] { 3, 4, 5 }, fps: 10f, loop: true));
            a.Play("walk");
            Assert.AreEqual(0, a.CurrentFrame);

            a.Advance(0.1); Assert.AreEqual(1, a.CurrentFrame);
            a.Advance(0.1); Assert.AreEqual(2, a.CurrentFrame);
            a.Advance(0.1); Assert.AreEqual(0, a.CurrentFrame, "volta ao início do clipe (3 frames)");
        }

        [TestMethod]
        public void Clipe_SemLoop_DisparaTerminouUmaVezESeguraNoUltimo()
        {
            var a = GridAnimator(10);
            a.AddClip(new SpriteClip("attack", new[] { 0, 1, 2 }, fps: 10f, loop: false));
            int done = 0; string? last = null;
            a.ClipFinished += n => { done++; last = n; };
            a.Play("attack");

            for (int i = 0; i < 10; i++) a.Advance(0.1);

            Assert.AreEqual(2, a.CurrentFrame, "segura no último frame do clipe");
            Assert.AreEqual(1, done, "dispara 'terminou' exatamente uma vez");
            Assert.AreEqual("attack", last);
            Assert.IsTrue(a.CurrentClipFinished);
        }

        [TestMethod]
        public void Play_TrocaDeClipe_Reinicia()
        {
            var a = GridAnimator(10);
            a.AddClip(new SpriteClip("walk", new[] { 0, 1, 2 }, 10f, true));
            a.AddClip(new SpriteClip("attack", new[] { 5, 6 }, 10f, false));
            a.Play("walk");
            a.Advance(0.1); a.Advance(0.1);
            Assert.AreEqual(2, a.CurrentFrame);

            a.Play("attack");
            Assert.AreEqual(0, a.CurrentFrame, "reinicia ao trocar de clipe");
            Assert.AreEqual("attack", a.CurrentClip);
        }

        private static (Scene, SpriteAnimator, SpriteAnimatorController, GameObject) BuildHero()
        {
            var scene = new Scene();
            var o = new GameObject("Hero");
            var anim = GridAnimator(12);
            anim.AddClip(SpriteClip.Range("idle", 0, 1, 6f, true));
            anim.AddClip(SpriteClip.Range("walk", 1, 4, 12f, true));
            anim.AddClip(SpriteClip.Range("jump", 5, 2, 10f, false));
            anim.AddClip(SpriteClip.Range("attack", 7, 3, 12f, false));
            o.AddComponent(anim);
            var ctrl = new SpriteAnimatorController();
            o.AddComponent(ctrl);
            scene.Add(o);
            return (scene, anim, ctrl, o);
        }

        [TestMethod]
        public void Controlador_EscolheClipePorMovimento_EViraSprite()
        {
            var (scene, anim, _, o) = BuildHero();
            scene.StartPlay();

            // Parado -> idle
            scene.Update(Frame(0.016));
            Assert.AreEqual("idle", anim.CurrentClip);

            // Move para a direita -> walk, sem flip (arte olha para a direita)
            o.Transform.Position += new Vector2(10, 0);
            scene.Update(Frame(0.016));
            Assert.AreEqual("walk", anim.CurrentClip);
            Assert.IsFalse(anim.FlipX);

            // Move para a esquerda -> flip
            o.Transform.Position += new Vector2(-10, 0);
            scene.Update(Frame(0.016));
            Assert.IsTrue(anim.FlipX, "vira ao andar para a esquerda");

            // Sobe rápido (no ar) -> jump
            o.Transform.Position += new Vector2(0, -40);
            scene.Update(Frame(0.016));
            Assert.AreEqual("jump", anim.CurrentClip);
        }

        [TestMethod]
        public void Controlador_Ataque_TocaUmaVezEVoltaAoIdle()
        {
            var (scene, anim, ctrl, _) = BuildHero();
            scene.StartPlay();

            // Frame 1: pressiona a ação de ataque (borda).
            GameInput.Update();
            GameInput.HoldAction("Action");
            scene.Update(Frame(0.05));
            Assert.IsTrue(ctrl.IsAttacking);
            Assert.AreEqual("attack", anim.CurrentClip);

            // Frames seguintes sem segurar: o ataque termina e volta ao idle.
            for (int i = 0; i < 20; i++)
            {
                GameInput.Update();
                scene.Update(Frame(0.05));
            }
            Assert.IsFalse(ctrl.IsAttacking);
            Assert.AreEqual("idle", anim.CurrentClip);
        }

        [TestMethod]
        public void Serializacao_RoundTrip_ClipesFlipEControlador()
        {
            var scene = new Scene();
            var o = new GameObject("Hero");
            var anim = GridAnimator(12);
            anim.AddClip(new SpriteClip("walk", new[] { 1, 2, 3 }, 12f, true));
            anim.AddClip(new SpriteClip("attack", new[] { 7, 8 }, 15f, false));
            anim.FlipX = true;
            anim.Play("walk");
            o.AddComponent(anim);
            o.AddComponent(new SpriteAnimatorController { AttackClip = "attack", ArtFacesRight = false });
            scene.Add(o);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            var la = loaded.Objects.First().Components.OfType<SpriteAnimator>().Single();
            Assert.AreEqual(2, la.Clips.Count);
            Assert.AreEqual("walk", la.CurrentClip);
            Assert.IsTrue(la.FlipX);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, la.Clips.First(c => c.Name == "walk").Frames);
            Assert.IsFalse(la.Clips.First(c => c.Name == "attack").Loop);

            var lc = loaded.Objects.First().Components.OfType<SpriteAnimatorController>().Single();
            Assert.AreEqual("attack", lc.AttackClip);
            Assert.IsFalse(lc.ArtFacesRight);
        }
    }
}
