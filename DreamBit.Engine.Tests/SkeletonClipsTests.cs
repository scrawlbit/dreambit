using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class SkeletonClipsTests
    {
        private static (GameObject root, GameObject bone, SkeletonAnimator anim) MakeRig()
        {
            var root = new GameObject("Heroi");
            var anim = new SkeletonAnimator();
            root.AddComponent(anim);
            var bone = new GameObject("Braco");
            bone.AddComponent(new Bone());
            root.AddChild(bone);
            return (root, bone, anim);
        }

        [TestMethod]
        public void ComecaComUmClipeDefault()
        {
            var (_, _, anim) = MakeRig();
            Assert.AreEqual(1, anim.Clips.Count);
            Assert.AreEqual("default", anim.CurrentClipName);
        }

        [TestMethod]
        public void KeyframesSaoPorClipe()
        {
            var (_, bone, anim) = MakeRig();

            // clipe default: 2 keyframes
            anim.CurrentClip.Name = "idle";
            bone.Transform.Position = new Vector2(0, 0); anim.CaptureKeyframe(0f);
            anim.CaptureKeyframe(1f);
            Assert.AreEqual(2, anim.KeyframeCount);

            // novo clipe "walk": vazio, independente
            anim.AddClip("walk");
            Assert.AreEqual("walk", anim.CurrentClipName);
            Assert.AreEqual(0, anim.KeyframeCount);

            anim.CaptureKeyframe(0.5f);
            Assert.AreEqual(1, anim.KeyframeCount);

            // volta para idle: continua com 2
            anim.CurrentClipName = "idle";
            Assert.AreEqual(2, anim.KeyframeCount);
        }

        [TestMethod]
        public void NaoRemoveOUltimoClipe()
        {
            var (_, _, anim) = MakeRig();
            Assert.IsFalse(anim.RemoveClip("default"), "não remove o único clipe");
            anim.AddClip("walk");
            Assert.IsTrue(anim.RemoveClip("walk"));
            Assert.AreEqual(1, anim.Clips.Count);
        }

        [TestMethod]
        public void Play_TrocaOClipeAtivo()
        {
            var (_, bone, anim) = MakeRig();
            anim.AddClip("walk");
            anim.CurrentClipName = "default";
            anim.Play("walk");
            Assert.AreEqual("walk", anim.CurrentClipName);
        }

        [TestMethod]
        public void Serializacao_PreservaClipesEAtivo()
        {
            var scene = new Scene();
            var (root, bone, anim) = MakeRig();

            anim.CurrentClip.Name = "idle";
            anim.Duration = 2f;
            bone.Transform.Position = new Vector2(0, 0); anim.CaptureKeyframe(0f);
            bone.Transform.Position = new Vector2(50, 0); anim.CaptureKeyframe(2f);

            anim.AddClip("walk");
            anim.Loop = false;
            bone.Transform.Position = new Vector2(0, 0); anim.CaptureKeyframe(0f);

            anim.CurrentClipName = "idle";
            scene.Add(root);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            var a = loaded.Objects.First().Components.OfType<SkeletonAnimator>().Single();

            CollectionAssert.AreEquivalent(new[] { "idle", "walk" }, a.ClipNames.ToArray());
            Assert.AreEqual("idle", a.CurrentClipName);
            Assert.AreEqual(2, a.KeyframeCount); // idle tem 2

            a.CurrentClipName = "walk";
            Assert.IsFalse(a.Loop);
            Assert.AreEqual(1, a.KeyframeCount);
        }

        [TestMethod]
        public void Serializacao_LeFormatoLegadoComoDefault()
        {
            // JSON no formato antigo (single-clip): Clips vazio, campos diretos.
            const string json = @"{""Name"":""Cena"",""Objects"":[{""Name"":""H"",
                ""Skeletons"":[{""Duration"":3.0,""Loop"":true,""Easing"":0,
                ""Keyframes"":[{""Time"":0.0,""Bones"":[]}],""Events"":[]}]}]}";

            var scene = SceneSerializer.LoadFromString(json);
            var a = scene.Objects.First().Components.OfType<SkeletonAnimator>().Single();
            Assert.AreEqual(1, a.Clips.Count);
            Assert.AreEqual("default", a.CurrentClipName);
            Assert.AreEqual(3f, a.Duration, 0.001f);
        }
    }
}
