using System;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using DreamBit.Engine.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using GameInput = DreamBit.Engine.Input.Input;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class UiTextFieldTests
    {
        private static GameTime Frame => new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.016));

        private static (Scene, UiTextField, GameObject) Build()
        {
            Screen.Set(800, 600);
            UiFocus.Clear();
            var scene = new Scene();
            var obj = new GameObject("Campo") { ScreenSpace = true };
            obj.Transform.Position = new Vector2(400, 300);
            obj.AddComponent(new UiTextField { Width = 200, Height = 34, MaxLength = 10, SendOnSubmit = "ok" });
            scene.Add(obj);
            return (scene, obj.Components.OfType<UiTextField>().Single(), obj);
        }

        // Injeta texto digitado no frame (o host faz PumpText por frame; simulamos aqui).
        private static void Type(Scene scene, string chars, Vector2 pointer)
        {
            foreach (var c in chars) GameInput.PushText(c);
            GameInput.PumpText();
            GameInput.SetPointer(pointer, false);
            scene.Update(Frame);
        }

        [TestMethod]
        public void ClicarFocaEDigitar()
        {
            var (scene, field, _) = Build();
            scene.StartPlay();

            // clica dentro => foca
            GameInput.SetPointer(new Vector2(400, 300), true); scene.Update(Frame);
            GameInput.SetPointer(new Vector2(400, 300), false); scene.Update(Frame);
            Assert.IsTrue(field.IsFocused);

            Type(scene, "Aria", new Vector2(400, 300));
            Assert.AreEqual("Aria", field.Text);
            GameInput.ClearPointerOverride();
        }

        [TestMethod]
        public void BackspaceApaga()
        {
            var (scene, field, _) = Build();
            scene.StartPlay();
            UiFocus.Set(field);

            Type(scene, "abc", new Vector2(400, 300));
            Assert.AreEqual("abc", field.Text);
            Type(scene, "\b", new Vector2(400, 300));
            Assert.AreEqual("ab", field.Text);
            GameInput.ClearPointerOverride();
        }

        [TestMethod]
        public void EnterConfirmaEDisparaEDesfoca()
        {
            var (scene, field, _) = Build();
            bool ok = false;
            scene.MessageSent += m => { if (m.Name == "ok") ok = true; };
            scene.StartPlay();
            UiFocus.Set(field);

            Type(scene, "oi\r", new Vector2(400, 300));
            Assert.AreEqual("oi", field.Text);
            Assert.IsTrue(ok, "Enter dispara a mensagem");
            Assert.IsFalse(field.IsFocused, "perde o foco ao confirmar");
            GameInput.ClearPointerOverride();
        }

        [TestMethod]
        public void RespeitaMaxLength()
        {
            var (scene, field, _) = Build(); // MaxLength = 10
            scene.StartPlay();
            UiFocus.Set(field);

            Type(scene, "0123456789ABC", new Vector2(400, 300));
            Assert.AreEqual(10, field.Text.Length);
            GameInput.ClearPointerOverride();
        }

        [TestMethod]
        public void SoODoFocoRecebe()
        {
            var (scene, field, _) = Build();
            scene.StartPlay();
            // sem foco: digitar não muda nada
            Type(scene, "xyz", new Vector2(10, 10));
            Assert.AreEqual(string.Empty, field.Text);
            GameInput.ClearPointerOverride();
        }

        [TestMethod]
        public void Serializacao_RoundTrip()
        {
            var scene = new Scene();
            var obj = new GameObject("C") { ScreenSpace = true };
            obj.AddComponent(new UiTextField { Text = "oi", Placeholder = "nome", Width = 180, MaxLength = 20, SendOnSubmit = "go" });
            scene.Add(obj);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            var f = loaded.Objects.First().Components.OfType<UiTextField>().Single();
            Assert.AreEqual("oi", f.Text);
            Assert.AreEqual("nome", f.Placeholder);
            Assert.AreEqual(20, f.MaxLength);
            Assert.AreEqual("go", f.SendOnSubmit);
        }
    }
}
