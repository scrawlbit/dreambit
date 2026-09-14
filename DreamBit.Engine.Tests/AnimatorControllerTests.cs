using System;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Xunit;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    public class AnimatorControllerTests
    {
        [Fact]
        public void EscolheOClipePeloEstado()
        {
            var scene = new Scene();
            var hero = new GameObject("Heroi") { Tag = "Player" };

            // Rig com clipes idle/walk (com 1 keyframe cada para poderem tocar).
            var skel = new SkeletonAnimator();
            skel.CurrentClip.Name = "idle";
            var bone = new GameObject("B"); bone.AddComponent(new Bone());
            hero.AddChild(bone);
            hero.AddComponent(skel);
            skel.CaptureKeyframe(0f);
            skel.AddClip("walk"); skel.CaptureKeyframe(0f);
            skel.CurrentClipName = "idle";

            hero.AddComponent(new PlatformerController { Gravity = 900f, HalfHeight = 24f, HalfWidth = 20f, UseKeyboard = false, HorizontalSpeed = 0f });
            hero.AddComponent(new AnimatorController { IdleClip = "idle", WalkClip = "walk", JumpClip = "jump" });

            // chão para pousar e ficar grounded
            var ground = new GameObject("Chao");
            ground.AddComponent(new BoxCollider { Size = new Vector2(400, 40) });
            ground.Transform.Position = new Vector2(0, 100);
            hero.Transform.Position = new Vector2(0, 0); // acima; cai e pousa
            scene.Add(ground);
            scene.Add(hero);

            scene.StartPlay();
            for (int i = 0; i < 30; i++) // deixa pousar
                scene.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.016)));
            Assert.Equal("idle", skel.CurrentClipName);

            // Passa a se mover: HorizontalSpeed != 0
            hero.Components.OfType<PlatformerController>().First().HorizontalSpeed = 100f;
            scene.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.016)));
            Assert.Equal("walk", skel.CurrentClipName);
        }

        [Fact]
        public void Serializacao_RoundTrip()
        {
            var scene = new Scene();
            var obj = new GameObject("O");
            obj.AddComponent(new AnimatorController { IdleClip = "parado", WalkClip = "correr", JumpClip = "pulo" });
            scene.Add(obj);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            var c = loaded.Objects.First().Components.OfType<AnimatorController>().Single();
            Assert.Equal("parado", c.IdleClip);
            Assert.Equal("correr", c.WalkClip);
            Assert.Equal("pulo", c.JumpClip);
        }
    }
}
