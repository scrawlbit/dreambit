using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    [TestClass]
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

        [TestMethod]
        public void CapturaKeyframes_DasPosesAtuais()
        {
            var (_, bone, anim) = MakeRig();

            bone.Transform.Rotation = 0f;
            anim.CaptureKeyframe(0f);

            bone.Transform.Rotation = 2f;
            anim.CaptureKeyframe(1f);

            Assert.AreEqual(2, anim.KeyframeCount);
        }

        [TestMethod]
        public void Sample_InterpolaAPoseNoMeio()
        {
            var (_, bone, anim) = MakeRig();

            bone.Transform.Rotation = 0f;
            anim.CaptureKeyframe(0f);
            bone.Transform.Rotation = 2f;
            anim.CaptureKeyframe(1f);

            anim.Sample(0.5f);
            Assert.AreEqual(1f, bone.Transform.Rotation, 0.001f, "meio do caminho entre 0 e 2");
        }

        [TestMethod]
        public void SetTime_ForaDosLimites_UsaPrimeiroOuUltimo()
        {
            var (_, bone, anim) = MakeRig();
            bone.Transform.Position = new Vector2(0, 0);
            anim.CaptureKeyframe(0f);
            bone.Transform.Position = new Vector2(100, 0);
            anim.CaptureKeyframe(1f);

            anim.SetTime(0f);
            Assert.AreEqual(0f, bone.Transform.Position.X, 0.01f);

            anim.SetTime(1f);
            Assert.AreEqual(100f, bone.Transform.Position.X, 0.01f);
        }

        [TestMethod]
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
            Assert.AreEqual(0f, bone.Transform.Rotation, 0.001f, "play começa amostrando t=0");

            scene.Update(new GameTime(System.TimeSpan.Zero, System.TimeSpan.FromSeconds(0.5)));
            Assert.AreEqual(0.5f, bone.Transform.Rotation, 0.001f);
        }

        [TestMethod]
        public void RemoveKeyframe_PorProximidade()
        {
            var (_, _, anim) = MakeRig();
            anim.CaptureKeyframe(0f);
            anim.CaptureKeyframe(0.5f);
            Assert.AreEqual(2, anim.KeyframeCount);

            Assert.IsTrue(anim.RemoveKeyframeNear(0.49f));
            Assert.AreEqual(1, anim.KeyframeCount);
        }

        [TestMethod]
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
            Assert.AreEqual(2f, animLoaded.Duration, 0.001f);
            Assert.AreEqual(2, animLoaded.KeyframeCount);

            // A pose do osso "Braço" no último keyframe deve ter rotação 1.5.
            var last = animLoaded.Keyframes.Last();
            Assert.IsTrue(last.Bones.ContainsKey("Braço"));
            Assert.AreEqual(1.5f, last.Bones["Braço"].Rotation, 0.001f);
        }
    }
}
