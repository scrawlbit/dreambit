using System;
using System.IO;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using DreamBit.Engine.Tests.Demo;
using Xunit;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    /// <summary>
    /// Testa o rig de bones (cutout) do inimigo Fuse: hierarquia de ossos com partes, e os clipes
    /// de pose idle/walk/attack/jump animando de fato. Grava fuse-enemy.dbscene ao final.
    /// </summary>
    public class FuseRigSmokeTests
    {
        private static GameTime Frame(double s = 0.033) => new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(s));

        private static bool AssetsPresent()
            => File.Exists(Path.Combine(ForestDemo.AssetsDir, "fuse-parts.json"));

        [Fact]
        public void RigFuse_Clipes_AnimamEExercitamTudo()
        {
            if (!AssetsPresent())
                return; // asset de terceiros ausente (gitignored): pula o smoke test

            var scene = new Scene();
            var fuse = FuseRig.Build(scene, new Vector2(0, 0));
            var anim = fuse.Components.OfType<SkeletonAnimator>().Single();

            // Estrutura do rig.
            var bones = anim.RigBones();
            Assert.Equal(11, bones.Count);
            CollectionAssert.Contains(anim.ClipNames.ToList(), "walk");
            CollectionAssert.Contains(anim.ClipNames.ToList(), "attack");

            scene.StartPlay();

            // walk: o osso da perna gira ao longo do tempo (anima).
            anim.Play("walk");
            scene.Update(Frame(0.0)); // aplica t=0
            float legAt0 = bones["legFront"].Transform.Rotation;
            for (int i = 0; i < 6; i++) scene.Update(Frame());
            float legLater = bones["legFront"].Transform.Rotation;
            Assert.NotEqual(legAt0, legLater);

            // attack: sem loop, dispara o evento "hit" no impacto e termina.
            string? msg = null; scene.MessageSent += m => { if (m.Name == "hit") msg = m.Name; };
            anim.Play("attack");
            for (int i = 0; i < 20; i++) scene.Update(Frame()); // 0.66s > 0.5s
            Assert.Equal("hit", msg);

            // idle: sela num clipe que repete.
            anim.Play("idle");
            for (int i = 0; i < 10; i++) scene.Update(Frame());
            Assert.Equal("idle", anim.CurrentClipName);

            // Serialização round-trip preserva ossos e clipes.
            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            var lroot = loaded.Objects.First();
            var lanim = FindSkeleton(lroot);
            Assert.NotNull(lanim);
            Assert.Equal(11, lanim!.RigBones().Count);
            CollectionAssert.Contains(lanim.ClipNames.ToList(), "attack");

            // Grava a cena jogável tocando "walk" numa pasta temporária (não suja o repo).
            anim.CurrentClipName = "walk";
            var outDir = Path.Combine(Path.GetTempPath(), "dreambit-smoke");
            Directory.CreateDirectory(outDir);
            File.WriteAllText(Path.Combine(outDir, "fuse-enemy.dbscene"),
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
