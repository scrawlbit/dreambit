using System.Collections.Generic;
using DreamBit.Engine.Animation;
using Xunit;

namespace DreamBit.Engine.Tests
{
    public class StateMachineTests
    {
        [Fact]
        public void TransicaoPorCondicao()
        {
            bool correr = false;
            var sm = new StateMachine()
                .AddState("idle")
                .AddState("run")
                .AddTransition("idle", "run", () => correr)
                .AddTransition("run", "idle", () => !correr);

            sm.Start("idle");
            sm.Update(0.016f);
            Assert.Equal("idle", sm.Current);

            correr = true;
            sm.Update(0.016f);
            Assert.Equal("run", sm.Current);

            correr = false;
            sm.Update(0.016f);
            Assert.Equal("idle", sm.Current);
        }

        [Fact]
        public void AnyTransitionTemPrioridade()
        {
            bool morrer = false;
            var sm = new StateMachine()
                .AddState("idle").AddState("run").AddState("dead")
                .AddTransition("idle", "run", () => true)
                .AddAnyTransition("dead", () => morrer);

            sm.Start("idle");
            morrer = true;
            sm.Update(0.016f);
            Assert.Equal("dead", sm.Current);
        }

        [Fact]
        public void CallbacksEnterUpdateExit()
        {
            var log = new List<string>();
            var sm = new StateMachine()
                .AddState("a",
                    onEnter: () => log.Add("enter-a"),
                    onUpdate: _ => log.Add("update-a"),
                    onExit: () => log.Add("exit-a"))
                .AddState("b", onEnter: () => log.Add("enter-b"))
                .AddTransition("a", "b", () => true);

            sm.Start("a");            // enter-a
            sm.Update(0.016f);        // troca a->b (exit-a, enter-b); update de b (sem callback)

            CollectionAssert.AreEqual(new[] { "enter-a", "exit-a", "enter-b" }, log);
        }

        [Fact]
        public void ChangedDispara()
        {
            string? de = null, para = null;
            var sm = new StateMachine().AddState("x").AddState("y");
            sm.Changed += (f, t) => { de = f; para = t; };

            sm.Start("x");
            sm.Go("y");
            Assert.Equal("x", de);
            Assert.Equal("y", para);
        }
    }
}
