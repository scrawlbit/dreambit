using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Dialogue;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using DreamBit.Engine.Saving;
using Xunit;

namespace DreamBit.Engine.Tests
{
    public class DialogueTests
    {
        private const string Json = @"{
          // comentários e vírgulas finais são aceitos
          ""id"": ""festival"",
          ""lines"": [
            { ""speaker"": ""Aloine"", ""text"": ""Você está bem?"", ""event"": ""aloine_sorri"" },
            { ""speaker"": ""Ruber"", ""text"": ""Graças a você."",
              ""choices"": [
                { ""text"": ""Pagar uma sidra"", ""goto"": ""sidra"", ""setFlag"": ""ofereceu_sidra"" },
                { ""text"": ""Ir embora"", ""goto"": ""end"" },
                { ""text"": ""(segredo)"", ""requireFlag"": ""nunca"" },
              ] },
            { ""label"": ""sidra"", ""speaker"": ""Aloine"", ""text"": ""Aceito!"", ""skipIfFlag"": ""ja_bebeu"" },
            { ""speaker"": ""Aloine"", ""text"": ""Só se for agora."", ""requireFlag"": ""ofereceu_sidra"", ""goto"": ""end"" },
            { ""speaker"": ""Ninguém"", ""text"": ""nunca mostrada"" }
          ]
        }";

        [Fact]
        public void Fluxo_ComEscolhaFlagsEEventos()
        {
            WorldState.Reset();
            var scene = new Scene();
            var player = new GameObject("Ruber");
            var pc = player.AddComponent(new PlatformerController());
            scene.Add(player);
            string? evento = null;
            scene.MessageSent += m => evento = m.Name;

            var asset = DialogueAsset.Parse(Json);
            DialogueRunner.Start(asset, scene);
            Assert.True(DialogueRunner.IsRunning);
            Assert.True(pc.InputLocked);
            Assert.Equal("Você está bem?", DialogueRunner.TextOf(DialogueRunner.Line!));
            scene.Update(new Microsoft.Xna.Framework.GameTime(System.TimeSpan.Zero, System.TimeSpan.FromSeconds(1.0 / 60))); // mensagens saem no Update
            Assert.Equal("aloine_sorri", evento);

            DialogueRunner.Advance();
            Assert.Equal(2, DialogueRunner.Choices.Count); // a terceira exige flag
            DialogueRunner.Advance(); // ignorado: precisa escolher
            Assert.Equal("Ruber", DialogueRunner.Line!.Speaker);

            DialogueRunner.Choose(0);
            Assert.True(WorldState.HasFlag("ofereceu_sidra"));
            Assert.Equal("Aceito!", DialogueRunner.Line!.Text);
            DialogueRunner.Advance();
            Assert.Equal("Só se for agora.", DialogueRunner.Line!.Text);
            DialogueRunner.Advance(); // goto end
            Assert.False(DialogueRunner.IsRunning);
            Assert.False(pc.InputLocked);
            WorldState.Reset();
        }

        [Fact]
        public void EscolherIrEmbora_TerminaNaHora()
        {
            DialogueRunner.Start(DialogueAsset.Parse(Json));
            DialogueRunner.Advance();
            DialogueRunner.Choose(1);
            Assert.False(DialogueRunner.IsRunning);
        }

        [Fact]
        public void IniciarPorRotulo_EPularPorFlag()
        {
            WorldState.Reset();
            WorldState.SetFlag("ja_bebeu");
            DialogueRunner.Start(DialogueAsset.Parse(Json), null, "sidra");
            Assert.Null(DialogueRunner.Line); // "sidra" pulada e a seguinte exige flag: termina
            Assert.False(DialogueRunner.IsRunning);
            WorldState.Reset();
        }

        [Fact]
        public void Localizacao_TemPrioridade()
        {
            Localization.Localizer.Load("pt", new System.Collections.Generic.Dictionary<string, string> { ["fala.oi"] = "Olá, Ruber" });
            Localization.Localizer.Language = "pt";
            var line = new DialogueLine { Text = "Hi", Key = "fala.oi" };
            Assert.Equal("Olá, Ruber", DialogueRunner.TextOf(line));
            Localization.Localizer.Clear();
        }

        [Fact]
        public void Fonts_SemTtf_QuebraLinhaComFontePixel()
        {
            Fonts.Clear();
            var wrapped = Fonts.Wrap(null, 14f, "um dois tres quatro cinco seis", 60f);
            Assert.Contains("\n", wrapped);
            Assert.All(wrapped.Split('\n'), l => Assert.True(Fonts.Measure(null, 14f, l).X <= 60f || !l.Contains(' ')));
        }

        [Fact]
        public void Fonts_RegistraTtf()
        {
            Fonts.Clear();
            var arial = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Fonts), "arial.ttf");
            if (!System.IO.File.Exists(arial)) return; // máquina sem a fonte
            Assert.True(Fonts.Register("ui", arial));
            Assert.True(Fonts.Has("ui"));
            Assert.NotNull(Fonts.Get("ui", 24));
            Fonts.Clear();
        }

        [Fact]
        public void Asset_IdaEVolta()
        {
            var asset = DialogueAsset.Parse(Json);
            var again = DialogueAsset.Parse(asset.ToJson());
            Assert.Equal(asset.Lines.Count, again.Lines.Count);
            Assert.Equal("sidra", again.Lines[2].Label);
            Assert.Equal(3, again.Lines[1].Choices.Count);
        }
    }
}
