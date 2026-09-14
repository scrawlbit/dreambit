using System.Linq;
using DreamBit.Engine.Audio;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Xunit;

namespace DreamBit.Engine.Tests
{
    public class AudioMixerTests
    {
        public AudioMixerTests() => AudioMixer.Reset();

        [Fact]
        public void EffectiveMultiplicaPeloMaster()
        {
            AudioMixer.SetVolume(AudioMixer.Master, 0.5f);
            AudioMixer.SetVolume(AudioMixer.Music, 0.8f);

            Assert.Equal(0.4f, AudioMixer.Effective(AudioMixer.Music), 0.0001f);
            Assert.Equal(0.5f, AudioMixer.Effective(AudioMixer.Master), 0.0001f);
        }

        [Fact]
        public void BusDesconhecidoVale1()
        {
            Assert.Equal(1f, AudioMixer.GetVolume("Ambiente"));
            AudioMixer.SetVolume("Ambiente", 0.25f);
            Assert.Equal(0.25f, AudioMixer.GetVolume("Ambiente"));
        }

        [Fact]
        public void VolumeClampa()
        {
            AudioMixer.SetVolume(AudioMixer.Sfx, 5f);
            Assert.Equal(1f, AudioMixer.GetVolume(AudioMixer.Sfx));
            AudioMixer.SetVolume(AudioMixer.Sfx, -3f);
            Assert.Equal(0f, AudioMixer.GetVolume(AudioMixer.Sfx));
        }

        [Fact]
        public void AudioSource_Serializacao_RoundTrip()
        {
            var scene = new Scene();
            var obj = new GameObject("Som");
            obj.AddComponent(new AudioSource { SoundPath = "m.wav", Volume = 0.7f, Loop = true, Bus = AudioMixer.Music });
            scene.Add(obj);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            var a = loaded.Objects.First().Components.OfType<AudioSource>().Single();
            Assert.Equal("m.wav", a.SoundPath);
            Assert.Equal(AudioMixer.Music, a.Bus);
            Assert.True(a.Loop);
        }
    }
}
