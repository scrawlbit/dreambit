using System;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class AnimationStateMachineTests
    {
        private static GameTime Frame => new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.016));

        private static (Scene, AnimationStateMachine, SpriteAnimator) Build()
        {
            var scene = new Scene();
            var o = new GameObject("Hero");
            var anim = new SpriteAnimator { FrameCount = 12 };
            anim.AddClip(SpriteClip.Range("idle", 0, 1));
            anim.AddClip(SpriteClip.Range("walk", 1, 4));
            anim.AddClip(SpriteClip.Range("attack", 6, 3, 12f, false));
            o.AddComponent(anim);

            var fsm = new AnimationStateMachine { DefaultState = "idle", BlendTime = 0f };
            fsm.AddState("idle", "idle").AddState("walk", "walk").AddState("attack", "attack");
            fsm.AddTransition("idle", "walk", "moving", AnimCondition.BoolTrue);
            fsm.AddTransition("walk", "idle", "moving", AnimCondition.BoolFalse);
            fsm.AddTransition("", "attack", "hit", AnimCondition.Trigger); // de qualquer estado
            o.AddComponent(fsm);
            scene.Add(o);
            return (scene, fsm, anim);
        }

        [TestMethod]
        public void TransicaoPorBool_TrocaEstadoEClipe()
        {
            var (scene, fsm, anim) = Build();
            scene.StartPlay();
            Assert.AreEqual("idle", fsm.CurrentState);
            Assert.AreEqual("idle", anim.CurrentClip);

            fsm.SetBool("moving", true);
            scene.Update(Frame);
            Assert.AreEqual("walk", fsm.CurrentState);
            Assert.AreEqual("walk", anim.CurrentClip);

            fsm.SetBool("moving", false);
            scene.Update(Frame);
            Assert.AreEqual("idle", fsm.CurrentState);
        }

        [TestMethod]
        public void Serializacao_RoundTrip_StateMachine()
        {
            var scene = new Scene();
            var o = new GameObject("H");
            var fsm = new AnimationStateMachine { DefaultState = "idle", BlendTime = 0.2f };
            fsm.AddState("idle", "idle").AddState("walk", "walk");
            fsm.AddTransition("idle", "walk", "moving", AnimCondition.BoolTrue);
            fsm.AddTransition("", "walk", "go", AnimCondition.Trigger);
            o.AddComponent(fsm);
            scene.Add(o);

            var e = DreamBit.Engine.Serialization.SceneSerializer
                .LoadFromString(DreamBit.Engine.Serialization.SceneSerializer.SaveToString(scene)).Objects.First();
            var lf = e.Components.OfType<AnimationStateMachine>().Single();
            Assert.AreEqual("idle", lf.DefaultState);
            Assert.AreEqual(2, lf.States.Count);
            Assert.AreEqual(2, lf.Transitions.Count);
            Assert.AreEqual(AnimCondition.Trigger, lf.Transitions.First(t => t.Parameter == "go").Condition);
        }

        [TestMethod]
        public void GatilhoDeQualquerEstado_VaiParaAttackEConsome()
        {
            var (scene, fsm, anim) = Build();
            scene.StartPlay();
            fsm.SetBool("moving", true);
            scene.Update(Frame); // -> walk

            fsm.SetTrigger("hit");
            scene.Update(Frame); // -> attack (de qualquer estado)
            Assert.AreEqual("attack", fsm.CurrentState);
            Assert.AreEqual("attack", anim.CurrentClip);

            // gatilho consumido: não volta a disparar sozinho
            scene.Update(Frame);
            Assert.AreEqual("attack", fsm.CurrentState);
        }
    }
}
