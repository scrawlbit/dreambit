using System.Linq;
using DreamBit.Engine.Audio;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class AudioMixerTests
    {
        [TestInitialize]
        public void Reset() => AudioMixer.Reset();

        [TestMethod]
        public void EffectiveMultiplicaPeloMaster()
        {
            AudioMixer.SetVolume(AudioMixer.Master, 0.5f);
            AudioMixer.SetVolume(AudioMixer.Music, 0.8f);

            Assert.AreEqual(0.4f, AudioMixer.Effective(AudioMixer.Music), 0.0001f, "0.8 * 0.5");
            Assert.AreEqual(0.5f, AudioMixer.Effective(AudioMixer.Master), 0.0001f, "Master é ele mesmo");
        }

        [TestMethod]
        public void BusDesconhecidoVale1()
        {
            Assert.AreEqual(1f, AudioMixer.GetVolume("Ambiente"));
            AudioMixer.SetVolume("Ambiente", 0.25f);
            Assert.AreEqual(0.25f, AudioMixer.GetVolume("Ambiente"));
        }

        [TestMethod]
        public void VolumeClampa()
        {
            AudioMixer.SetVolume(AudioMixer.Sfx, 5f);
            Assert.AreEqual(1f, AudioMixer.GetVolume(AudioMixer.Sfx));
            AudioMixer.SetVolume(AudioMixer.Sfx, -3f);
            Assert.AreEqual(0f, AudioMixer.GetVolume(AudioMixer.Sfx));
        }

        [TestMethod]
        public void AudioSource_Serializacao_RoundTrip()
        {
            var scene = new Scene();
            var obj = new GameObject("Som");
            obj.AddComponent(new AudioSource { SoundPath = "m.wav", Volume = 0.7f, Loop = true, Bus = AudioMixer.Music });
            scene.Add(obj);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            var a = loaded.Objects.First().Components.OfType<AudioSource>().Single();
            Assert.AreEqual("m.wav", a.SoundPath);
            Assert.AreEqual(AudioMixer.Music, a.Bus);
            Assert.IsTrue(a.Loop);
        }
    }
}
