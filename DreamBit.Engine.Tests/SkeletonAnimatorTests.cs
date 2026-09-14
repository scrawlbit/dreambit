using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Xunit;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    public class SkeletonAnimatorTests
    {
        // Cria um rig: raiz com SkeletonAnimator + um osso filho chamado "Braço".
        private static (GameObject root, GameObject bone, SkeletonAnimator anim) MakeRig()
        {
            var root = new GameObject("Herói");
            var anim = new SkeletonAnimator { Duration = 1f, Loop = false };
            root.AddComponent(anim);

            var bone = new GameObject("Braço");
            bone.AddComponent(new Bone());
            root.AddChild(bone);

            return (root, bone, anim);
        }

        [Fact]
        public void CapturaKeyframes_DasPosesAtuais()
        {
            var (_, bone, anim) = MakeRig();

            bone.Transform.Rotation = 0f;
            anim.CaptureKeyframe(0f);

            bone.Transform.Rotation = 2f;
            anim.CaptureKeyframe(1f);

            Assert.Equal(2, anim.KeyframeCount);
        }

        [Fact]
        public void Sample_InterpolaAPoseNoMeio()
        {
            var (_, bone, anim) = MakeRig();

            bone.Transform.Rotation = 0f;
            anim.CaptureKeyframe(0f);
            bone.Transform.Rotation = 2f;
            anim.CaptureKeyframe(1f);

            anim.Sample(0.5f);
            Assert.Equal(1f, bone.Transform.Rotation, 0.001f);
        }

        [Fact]
        public void SetTime_ForaDosLimites_UsaPrimeiroOuUltimo()
        {
            var (_, bone, anim) = MakeRig();
            bone.Transform.Position = new Vector2(0, 0);
            anim.CaptureKeyframe(0f);
            bone.Transform.Position = new Vector2(100, 0);
            anim.CaptureKeyframe(1f);

            anim.SetTime(0f);
            Assert.Equal(0f, bone.Transform.Position.X, 0.01f);

            anim.SetTime(1f);
            Assert.Equal(100f, bone.Transform.Position.X, 0.01f);
        }

        [Fact]
        public void Play_AvancaEAplicaAPose()
        {
            var scene = new Scene();
            var (root, bone, anim) = MakeRig();
            bone.Transform.Rotation = 0f;
            anim.CaptureKeyframe(0f);
            bone.Transform.Rotation = 1f;
            anim.CaptureKeyframe(1f);
            bone.Transform.Rotation = 5f; // pose qualquer antes do play
            scene.Add(root);

            scene.StartPlay();
            Assert.Equal(0f, bone.Transform.Rotation, 0.001f);

            scene.Update(new GameTime(System.TimeSpan.Zero, System.TimeSpan.FromSeconds(0.5)));
            Assert.Equal(0.5f, bone.Transform.Rotation, 0.001f);
        }

        [Fact]
        public void RemoveKeyframe_PorProximidade()
        {
            var (_, _, anim) = MakeRig();
            anim.CaptureKeyframe(0f);
            anim.CaptureKeyframe(0.5f);
            Assert.Equal(2, anim.KeyframeCount);

            Assert.True(anim.RemoveKeyframeNear(0.49f));
            Assert.Equal(1, anim.KeyframeCount);
        }

        [Fact]
        public void Serializacao_PreservaClipeEKeyframes()
        {
            var scene = new Scene();
            var (root, bone, anim) = MakeRig();
            anim.Duration = 2f;
            bone.Transform.Rotation = 0.5f;
            anim.CaptureKeyframe(0f);
            bone.Transform.Rotation = 1.5f;
            anim.CaptureKeyframe(2f);
            scene.Add(root);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));

            var rootLoaded = loaded.Objects.First();
            var animLoaded = rootLoaded.Components.OfType<SkeletonAnimator>().Single();
            Assert.Equal(2f, animLoaded.Duration, 0.001f);
            Assert.Equal(2, animLoaded.KeyframeCount);

            // A pose do osso "Braço" no último keyframe deve ter rotação 1.5.
            var last = animLoaded.Keyframes.Last();
            Assert.True(last.Bones.ContainsKey("Braço"));
            Assert.Equal(1.5f, last.Bones["Braço"].Rotation, 0.001f);
        }
    }
}
