using System;
using System.Collections.Generic;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Messaging;
using DreamBit.Engine.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class SkeletonEasingEventsTests
    {
        private static (GameObject root, GameObject bone, SkeletonAnimator anim) MakeRig()
        {
            var root = new GameObject("Heroi");
            var anim = new SkeletonAnimator { Duration = 1f, Loop = false };
            root.AddComponent(anim);
            var bone = new GameObject("Braco");
            bone.AddComponent(new Bone());
            root.AddChild(bone);
            return (root, bone, anim);
        }

        [TestMethod]
        public void Easing_InOut_NaoEhLinearNoMeio()
        {
            // Usa posição (interpola sem wrap de ângulo) para comparar as curvas.
            var (_, bone, anim) = MakeRig();
            bone.Transform.Position = new Vector2(0, 0); anim.CaptureKeyframe(0f);
            bone.Transform.Position = new Vector2(100, 0); anim.CaptureKeyframe(1f);

            anim.Easing = Scrawlbit.EasingMode.Linear;
            anim.Sample(0.25f);
            float linear = bone.Transform.Position.X;

            anim.Easing = Scrawlbit.EasingMode.InOut;
            anim.Sample(0.25f);
            float eased = bone.Transform.Position.X;

            Assert.AreEqual(25f, linear, 0.01f);           // 0.25 * 100
            Assert.AreEqual(15.625f, eased, 0.01f);        // smoothstep(0.25) * 100
        }

        [TestMethod]
        public void Eventos_DisparamNoTempoEForwardamAoBarramento()
        {
            var scene = new Scene();
            var (root, bone, anim) = MakeRig();
            bone.Transform.Rotation = 0f; anim.CaptureKeyframe(0f);
            bone.Transform.Rotation = 1f; anim.CaptureKeyframe(1f);
            anim.SetEvents(new[] { (0.5f, "passo") });

            var recebidas = new List<string>();
            anim.AnimationEvent += recebidas.Add;

            // Um listener na cena reage à mensagem "passo".
            var flag = new GameObject("Flag");
            var listener = new MessageListener { Message = "passo", Reaction = MessageReaction.Hide };
            flag.AddComponent(listener);
            scene.Add(root);
            scene.Add(flag);

            scene.StartPlay();
            for (int i = 0; i < 40; i++) // ~0.64s, cruza 0.5
                scene.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.016)));

            Assert.AreEqual(1, recebidas.Count(x => x == "passo"), "evento dispara uma vez ao cruzar 0.5");
            Assert.IsFalse(flag.IsVisible, "o evento chegou ao barramento e o listener reagiu");
        }

        [TestMethod]
        public void Serializacao_PreservaEasingEEventos()
        {
            var scene = new Scene();
            var (root, bone, anim) = MakeRig();
            anim.Easing = Scrawlbit.EasingMode.Out;
            bone.Transform.Rotation = 0f; anim.CaptureKeyframe(0f);
            bone.Transform.Rotation = 1f; anim.CaptureKeyframe(1f);
            anim.SetEvents(new[] { (0.25f, "a"), (0.75f, "b") });
            scene.Add(root);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            var a = loaded.Objects.First().Components.OfType<SkeletonAnimator>().Single();
            Assert.AreEqual(Scrawlbit.EasingMode.Out, a.Easing);
            Assert.AreEqual(2, a.Events.Count());
            Assert.AreEqual("a", a.Events.OrderBy(e => e.Time).First().Name);
        }
    }
}
