using System.Collections.Generic;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Localization;
using DreamBit.Engine.Serialization;
using Xunit;

namespace DreamBit.Engine.Tests
{
    public class LocalizationTests
    {
        public LocalizationTests()
        {
            Localizer.Clear();
            Localizer.Language = "pt";
            Localizer.FallbackLanguage = "en";
        }

        [Fact]
        public void ResolvePeloIdiomaAtual()
        {
            Localizer.Load("pt", new Dictionary<string, string> { ["play"] = "Jogar" });
            Localizer.Load("en", new Dictionary<string, string> { ["play"] = "Play" });

            Localizer.Language = "pt";
            Assert.Equal("Jogar", Localizer.Get("play"));

            Localizer.Language = "en";
            Assert.Equal("Play", Localizer.Get("play"));
        }

        [Fact]
        public void CaiNoFallbackDepoisNaChave()
        {
            Localizer.Load("en", new Dictionary<string, string> { ["settings"] = "Settings" });
            Localizer.Language = "pt"; // sem tabela pt

            Assert.Equal("Settings", Localizer.Get("settings"));
            Assert.Equal("inexistente", Localizer.Get("inexistente"));
        }

        [Fact]
        public void CarregaJson()
        {
            Localizer.LoadJson("pt", "{\"hi\":\"Oi\",\"bye\":\"Tchau\"}");
            Assert.Equal("Oi", Localizer.Get("hi"));
            Assert.Equal("Tchau", Localizer.Get("bye"));
        }

        [Fact]
        public void TextRenderer_UsaLocKeyQuandoPresente()
        {
            Localizer.Load("pt", new Dictionary<string, string> { ["menu.play"] = "JOGAR" });
            var t = new TextRenderer { Text = "fallback", LocKey = "menu.play" };
            Assert.Equal("JOGAR", t.DisplayText);

            t.LocKey = "";
            Assert.Equal("fallback", t.DisplayText);
        }

        [Fact]
        public void TextRenderer_Serializacao_RoundTrip()
        {
            var scene = new Scene();
            var obj = new GameObject("Botao");
            obj.AddComponent(new TextRenderer { Text = "x", LocKey = "menu.play" });
            scene.Add(obj);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            var t = loaded.Objects.First().Components.OfType<TextRenderer>().Single();
            Assert.Equal("menu.play", t.LocKey);
        }
    }
}
