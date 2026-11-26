using DreamBit.Engine.Audio;
using Xunit;

namespace DreamBit.Engine.Tests
{
    public sealed class SoundPlayerTests
    {
        [Fact]
        public void VolumeEfetivo_MultiplicaBusEMaster()
        {
            AudioMixer.Reset();
            AudioMixer.SetVolume(AudioMixer.Master, 0.5f);
            AudioMixer.SetVolume(AudioMixer.Sfx, 0.4f);
            Assert.Equal(0.5f * 0.4f * 0.8f, SoundPlayer.EffectiveVolume(0.8f), 4);
            AudioMixer.Reset();
        }

        [Fact]
        public void IntervaloMinimo_NaoEmpilhaOMesmoSom()
        {
            SoundPlayer.Reset();
            Assert.True(SoundPlayer.TryClaim("a.wav", 0.1f));
            Assert.False(SoundPlayer.TryClaim("a.wav", 0.1f));
            Assert.True(SoundPlayer.TryClaim("b.wav", 0.1f));
            SoundPlayer.Tick(0.2f);
            Assert.True(SoundPlayer.TryClaim("a.wav", 0.1f));
            SoundPlayer.Reset();
        }

        [Fact]
        public void SemArquivoOuMudo_NaoToca()
        {
            SoundPlayer.Reset();
            Assert.False(SoundPlayer.Play(null));
            Assert.False(SoundPlayer.Play("nao/existe.wav"));
            AudioMixer.SetVolume(AudioMixer.Sfx, 0f);
            Assert.False(SoundPlayer.Play("nao/existe.wav"));
            AudioMixer.Reset();
        }
    }
}
