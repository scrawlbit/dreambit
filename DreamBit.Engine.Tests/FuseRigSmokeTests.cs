using System;
using System.IO;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using DreamBit.Engine.Tests.Demo;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    /// <summary>
    /// Testa o rig de bones (cutout) do inimigo Fuse: hierarquia de ossos com partes, e os clipes
    /// de pose idle/walk/attack/jump animando de fato. Grava fuse-enemy.dbscene ao final.
    /// </summary>
    [TestClass]
    public class FuseRigSmokeTests
    {
        private static GameTime Frame(double s = 0.033) => new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(s));

        private static bool AssetsPresent()
            => File.Exists(Path.Combine(ForestDemo.AssetsDir, "fuse-parts.json"));

        [TestMethod]
        public void RigFuse_Clipes_AnimamEExercitamTudo()
        {
            if (!AssetsPresent())
                Assert.Inconclusive("DemoAssets/fuse-parts.json ausente.");

            var scene = new Scene();
            var fuse = FuseRig.Build(scene, new Vector2(0, 0));
            var anim = fuse.Components.OfType<SkeletonAnimator>().Single();

            // Estrutura do rig.
            var bones = anim.RigBones();
            Assert.AreEqual(11, bones.Count, "11 ossos no rig");
            CollectionAssert.Contains(anim.ClipNames.ToList(), "walk");
            CollectionAssert.Contains(anim.ClipNames.ToList(), "attack");

            scene.StartPlay();

            // walk: o osso da perna gira ao longo do tempo (anima).
            anim.Play("walk");
            scene.Update(Frame(0.0)); // aplica t=0
            float legAt0 = bones["legFront"].Transform.Rotation;
            for (int i = 0; i < 6; i++) scene.Update(Frame());
            float legLater = bones["legFront"].Transform.Rotation;
            Assert.AreNotEqual(legAt0, legLater, 0.001f, "a perna anima no clipe walk");

            // attack: sem loop, dispara o evento "hit" no impacto e termina.
            string? msg = null; scene.MessageSent += m => { if (m.Name == "hit") msg = m.Name; };
            anim.Play("attack");
            for (int i = 0; i < 20; i++) scene.Update(Frame()); // 0.66s > 0.5s
            Assert.AreEqual("hit", msg, "clipe de ataque dispara o evento de golpe no frame");

            // idle: sela num clipe que repete.
            anim.Play("idle");
            for (int i = 0; i < 10; i++) scene.Update(Frame());
            Assert.AreEqual("idle", anim.CurrentClipName);

            // Serialização round-trip preserva ossos e clipes.
            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            var lroot = loaded.Objects.First();
            var lanim = FindSkeleton(lroot);
            Assert.IsNotNull(lanim);
            Assert.AreEqual(11, lanim!.RigBones().Count);
            CollectionAssert.Contains(lanim.ClipNames.ToList(), "attack");

            // Grava a cena jogável tocando "walk".
            anim.CurrentClipName = "walk";
            File.WriteAllText(Path.Combine(ForestDemo.AssetsDir, "fuse-enemy.dbscene"),
                SceneSerializer.SaveToString(scene));
        }

        private static SkeletonAnimator? FindSkeleton(GameObject obj)
        {
            var s = obj.Components.OfType<SkeletonAnimator>().FirstOrDefault();
            if (s != null) return s;
            foreach (var c in obj.Children)
            {
                var found = FindSkeleton(c);
                if (found != null) return found;
            }
            return null;
        }
    }
}
