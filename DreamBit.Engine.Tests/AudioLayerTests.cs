using System;
using System.Linq;
using DreamBit.Engine.Audio;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class AudioLayerTests
    {
        private static GameTime Frame(double s) => new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(s));

        [TestMethod]
        public void Ducking_AbaixaERecuperaOBus()
        {
            AudioMixer.Reset();
            Assert.AreEqual(1f, AudioMixer.Effective(AudioMixer.Music), 0.001f);

            AudioMixer.Duck(AudioMixer.Music, factor: 0.2f, duration: 1f);
            Assert.AreEqual(0.2f, AudioMixer.Effective(AudioMixer.Music), 0.05f, "abaixa na hora");

            AudioMixer.Tick(0.5f);
            Assert.IsTrue(AudioMixer.Effective(AudioMixer.Music) > 0.4f, "recupera pela metade");

            AudioMixer.Tick(0.6f);
            Assert.AreEqual(1f, AudioMixer.Effective(AudioMixer.Music), 0.001f, "recupera por completo");
        }

        [TestMethod]
        public void FadeTo_TransicionaOVolumeDaFonte()
        {
            var scene = new Scene();
            var o = new GameObject("Musica");
            var src = new AudioSource { Volume = 1f, PlayOnStart = false };
            o.AddComponent(src);
            scene.Add(o);
            scene.StartPlay();

            src.FadeTo(0f, 0.5f); // fade-out em 0.5s
            for (int i = 0; i < 8; i++) scene.Update(Frame(0.033));
            Assert.IsTrue(src.Volume < 0.6f, "volume caindo durante o fade");
            for (int i = 0; i < 10; i++) scene.Update(Frame(0.033));
            Assert.AreEqual(0f, src.Volume, 0.01f, "chega ao alvo do fade");
        }
    }
}
