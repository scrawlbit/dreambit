using System;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using Xunit;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    public class AnimationBlendTests
    {
        private static GameTime Frame(double s) => new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(s));

        [Fact]
        public void Sprite_Crossfade_TransicaoDePeso()
        {
            var a = new SpriteAnimator { FrameCount = 12, Fps = 10f };
            a.AddClip(SpriteClip.Range("idle", 0, 2));
            a.AddClip(SpriteClip.Range("walk", 4, 4));
            a.Play("idle");
            a.Advance(0.1);

            a.Play("walk", blendTime: 0.2f);
            Assert.Equal(0f, a.BlendWeight, 0.001f);
            a.Advance(0.1); // metade do blend
            Assert.Equal(0.5f, a.BlendWeight, 0.05f);
            a.Advance(0.2); // passa do fim
            Assert.Equal(1f, a.BlendWeight, 0.001f);
        }

        private static (Scene, SkeletonAnimator, GameObject) BuildRig()
        {
            var scene = new Scene();
            var root = new GameObject("Rig");
            var anim = new SkeletonAnimator();
            root.AddComponent(anim);
            var bone = new GameObject("b");
            bone.AddComponent(new Bone());
            root.AddChild(bone);
            scene.Add(root);

            anim.AddClip("a"); anim.Duration = 1f; anim.Loop = true;
            bone.Transform.Rotation = 0f; anim.CaptureKeyframe(0f);
            bone.Transform.Rotation = 0f; anim.CaptureKeyframe(1f);
            anim.AddClip("b"); anim.Duration = 1f; anim.Loop = true;
            bone.Transform.Rotation = 1f; anim.CaptureKeyframe(0f); // pose bem diferente
            bone.Transform.Rotation = 1f; anim.CaptureKeyframe(1f);
            return (scene, anim, bone);
        }

        [Fact]
        public void Skeleton_Crossfade_InterpolaEntreClipes()
        {
            var (scene, anim, bone) = BuildRig();
            anim.CurrentClipName = "a";
            scene.StartPlay(); // pose "a": rotação 0

            anim.Play("b", blendTime: 0.4f); // alvo rotação 1
            Assert.True(anim.IsBlending);

            scene.Update(Frame(0.2)); // meio do blend -> ~0.5
            Assert.True(bone.Transform.Rotation > 0.2f && bone.Transform.Rotation < 0.8f, $"pose intermediária durante o crossfade (rot={bone.Transform.Rotation:0.00})");

            for (int i = 0; i < 10; i++) scene.Update(Frame(0.1));
            Assert.False(anim.IsBlending, "blend termina");
            Assert.Equal(1f, bone.Transform.Rotation, 0.05f);
        }
    }
}
