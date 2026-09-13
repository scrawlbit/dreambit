using System.Collections.Generic;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Localization;
using DreamBit.Engine.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class LocalizationTests
    {
        [TestInitialize]
        public void Reset()
        {
            Localizer.Clear();
            Localizer.Language = "pt";
            Localizer.FallbackLanguage = "en";
        }

        [TestMethod]
        public void ResolvePeloIdiomaAtual()
        {
            Localizer.Load("pt", new Dictionary<string, string> { ["play"] = "Jogar" });
            Localizer.Load("en", new Dictionary<string, string> { ["play"] = "Play" });

            Localizer.Language = "pt";
            Assert.AreEqual("Jogar", Localizer.Get("play"));

            Localizer.Language = "en";
            Assert.AreEqual("Play", Localizer.Get("play"));
        }

        [TestMethod]
        public void CaiNoFallbackDepoisNaChave()
        {
            Localizer.Load("en", new Dictionary<string, string> { ["settings"] = "Settings" });
            Localizer.Language = "pt"; // sem tabela pt

            Assert.AreEqual("Settings", Localizer.Get("settings"), "usa fallback en");
            Assert.AreEqual("inexistente", Localizer.Get("inexistente"), "sem tradução => a própria chave");
        }

        [TestMethod]
        public void CarregaJson()
        {
            Localizer.LoadJson("pt", "{\"hi\":\"Oi\",\"bye\":\"Tchau\"}");
            Assert.AreEqual("Oi", Localizer.Get("hi"));
            Assert.AreEqual("Tchau", Localizer.Get("bye"));
        }

        [TestMethod]
        public void TextRenderer_UsaLocKeyQuandoPresente()
        {
            Localizer.Load("pt", new Dictionary<string, string> { ["menu.play"] = "JOGAR" });
            var t = new TextRenderer { Text = "fallback", LocKey = "menu.play" };
            Assert.AreEqual("JOGAR", t.DisplayText);

            t.LocKey = "";
            Assert.AreEqual("fallback", t.DisplayText, "sem chave => usa Text");
        }

        [TestMethod]
        public void TextRenderer_Serializacao_RoundTrip()
        {
            var scene = new Scene();
            var obj = new GameObject("Botao");
            obj.AddComponent(new TextRenderer { Text = "x", LocKey = "menu.play" });
            scene.Add(obj);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            var t = loaded.Objects.First().Components.OfType<TextRenderer>().Single();
            Assert.AreEqual("menu.play", t.LocKey);
        }
    }
}
