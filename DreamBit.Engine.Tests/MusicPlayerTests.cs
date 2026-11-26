using System.Linq;
using DreamBit.Engine.Audio;
using Xunit;

namespace DreamBit.Engine.Tests
{
    /// <summary>Música de fundo independente de cena: crossfade, mesma faixa não reinicia, parar com fade, volume do bus.
    /// Sem som (Enabled = false): só a lógica dos fades.</summary>
    public sealed class MusicPlayerTests
    {
        private static void Fresh()
        {
            MusicPlayer.Enabled = false;
            MusicPlayer.Reset();
            AudioMixer.Reset();
        }

        private static MusicPlayer.Voice V(string path) => MusicPlayer.Voices.First(v => v.Path == path);

        [Fact]
        public void Tocar_EntraComFade()
        {
            Fresh();
            Assert.False(MusicPlayer.Play("a.wav", fade: 1f));   // sem som, mas a faixa vira a atual
            Assert.Equal("a.wav", MusicPlayer.Current);
            Assert.Equal(0f, V("a.wav").Level, 3);
            MusicPlayer.Tick(0.5f);
            Assert.Equal(0.5f, V("a.wav").Level, 3);
            MusicPlayer.Tick(0.6f);
            Assert.Equal(1f, V("a.wav").Level, 3);
            MusicPlayer.Reset();
        }

        [Fact]
        public void TrocarDeFaixa_FazCrossfadeEEsqueceAAntiga()
        {
            Fresh();
            MusicPlayer.Play("a.wav", fade: 0f);
            MusicPlayer.Play("b.wav", fade: 2f);
            MusicPlayer.Tick(1f);
            Assert.Equal(0.5f, V("a.wav").Level, 3);
            Assert.Equal(0.5f, V("b.wav").Level, 3);
            Assert.True(V("a.wav").FadingOut);
            MusicPlayer.Tick(1.01f);
            Assert.Single(MusicPlayer.Voices);
            Assert.Equal("b.wav", MusicPlayer.Current);
            Assert.Equal(1f, V("b.wav").Level, 3);
            MusicPlayer.Reset();
        }

        [Fact]
        public void MesmaFaixa_NaoReiniciaOFade()
        {
            Fresh();
            MusicPlayer.Play("a.wav", fade: 1f);
            MusicPlayer.Tick(0.5f);
            MusicPlayer.Play("a.wav", fade: 1f);
            Assert.Single(MusicPlayer.Voices);
            Assert.Equal(0.5f, V("a.wav").Level, 3);
            MusicPlayer.Tick(0.5f);
            Assert.Equal(1f, V("a.wav").Level, 3);
            MusicPlayer.Reset();
        }

        [Fact]
        public void FaixaSumindo_VoltaDeOndeEstava()
        {
            Fresh();
            MusicPlayer.Play("sala.wav", fade: 0f);
            MusicPlayer.Play("chefe.wav", fade: 1f);
            MusicPlayer.Tick(0.25f);                       // sala em 0,75 e sumindo
            MusicPlayer.Play("sala.wav", fade: 1f);
            Assert.Equal(2, MusicPlayer.Voices.Count);
            Assert.False(V("sala.wav").FadingOut);
            Assert.True(V("chefe.wav").FadingOut);
            MusicPlayer.Tick(0.5f);
            Assert.Equal(0.875f, V("sala.wav").Level, 3);  // 0,75 → 1 em 1 s
            MusicPlayer.Reset();
        }

        [Fact]
        public void Parar_SomeComFade_ESemFadeCortaJa()
        {
            Fresh();
            MusicPlayer.Play("a.wav", fade: 0f);
            MusicPlayer.Stop(1f);
            Assert.Null(MusicPlayer.Current);
            MusicPlayer.Tick(0.5f);
            Assert.Equal(0.5f, V("a.wav").Level, 3);
            MusicPlayer.Tick(0.6f);
            Assert.Empty(MusicPlayer.Voices);

            MusicPlayer.Play("b.wav", fade: 0f);
            MusicPlayer.Stop(0f);
            Assert.Empty(MusicPlayer.Voices);
            MusicPlayer.Play(null);
            Assert.Null(MusicPlayer.Current);
            MusicPlayer.Reset();
        }

        [Fact]
        public void Volume_NivelVezesFaixaVezesBusEMaster()
        {
            Fresh();
            AudioMixer.SetVolume(AudioMixer.Master, 0.5f);
            AudioMixer.SetVolume(AudioMixer.Music, 0.4f);
            Assert.Equal(0.5f * 0.4f * 0.8f * 0.5f, MusicPlayer.EffectiveVolume(0.5f, 0.8f), 4);
            AudioMixer.SetVolume(AudioMixer.Music, 0f);
            Assert.Equal(0f, MusicPlayer.EffectiveVolume(1f), 4);
            MusicPlayer.Play("a.wav", fade: 0f, volume: 0.6f);
            Assert.Equal(0.6f, V("a.wav").Volume, 3);
            Assert.False(V("a.wav").Audible);
            AudioMixer.Reset();
            MusicPlayer.Reset();
        }

        [Fact]
        public void ArquivoInexistente_NaoQuebra()
        {
            Fresh();
            MusicPlayer.Enabled = true;
            Assert.False(MusicPlayer.Play("nao/existe.wav", fade: 0f));
            Assert.Equal("nao/existe.wav", MusicPlayer.Current);
            MusicPlayer.Tick(0.1f);
            MusicPlayer.Reset();
            MusicPlayer.Enabled = false;
        }
    }
}
