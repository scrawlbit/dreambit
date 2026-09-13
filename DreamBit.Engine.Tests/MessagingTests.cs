using System;
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
    public class MessagingTests
    {
        private static GameTime Frame => new(TimeSpan.Zero, TimeSpan.FromSeconds(0.016));

        [TestMethod]
        public void Send_EntregaAosListenersNoUpdate()
        {
            var scene = new Scene();
            var porta = new GameObject("Porta");
            var listener = new MessageListener { Message = "abrir", Reaction = MessageReaction.Hide };
            porta.AddComponent(listener);
            scene.Add(porta);

            scene.Send("abrir");
            Assert.IsTrue(porta.IsVisible, "ainda não despachou");

            scene.Update(Frame);
            Assert.IsFalse(porta.IsVisible, "listener reagiu escondendo o dono");
        }

        [TestMethod]
        public void Listener_IgnoraMensagemDiferente()
        {
            var scene = new Scene();
            var obj = new GameObject { };
            var listener = new MessageListener { Message = "abrir", Reaction = MessageReaction.Destroy };
            obj.AddComponent(listener);
            scene.Add(obj);

            scene.Send("outra");
            scene.Update(Frame);

            Assert.AreEqual(1, scene.Objects.Count, "mensagem diferente não deve reagir");
        }

        [TestMethod]
        public void TriggerZone_EnviaMensagemAoTocar()
        {
            var scene = new Scene();

            var player = new GameObject("Player") { Tag = "Player" };
            player.AddComponent(new SpriteRenderer { Size = new Vector2(20, 20) });
            scene.Add(player);

            var botao = new GameObject("Botao");
            botao.AddComponent(new TriggerZone { Size = new Vector2(40, 40), TargetTag = "Player", DestroyOnEnter = false, SendOnEnter = "abrir" });
            scene.Add(botao);

            var porta = new GameObject("Porta");
            porta.AddComponent(new MessageListener { Message = "abrir", Reaction = MessageReaction.Hide });
            scene.Add(porta);

            scene.StartPlay();
            scene.Update(Frame); // trigger detecta e envia; despacha no mesmo Update

            Assert.IsFalse(porta.IsVisible, "tocar o botão abriu a porta pela mensagem");
        }

        [TestMethod]
        public void Serializacao_PreservaListenerETriggerSend()
        {
            var scene = new Scene();
            var obj = new GameObject("X");
            obj.AddComponent(new MessageListener { Message = "ping", Reaction = MessageReaction.ToggleVisible });
            obj.AddComponent(new TriggerZone { SendOnEnter = "ping", DestroyOnEnter = false });
            scene.Add(obj);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            var lo = loaded.Objects.First();
            var listener = lo.Components.OfType<MessageListener>().Single();
            Assert.AreEqual("ping", listener.Message);
            Assert.AreEqual(MessageReaction.ToggleVisible, listener.Reaction);
            Assert.AreEqual("ping", lo.Components.OfType<TriggerZone>().Single().SendOnEnter);
        }
    }
}
