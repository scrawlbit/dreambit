using System;
using DreamBit.Engine.Audio;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class LayeredMusicTests
    {
        private static GameTime Frame(double s) => new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(s));

        private static void CameraAtOrigin()
        {
            Screen.Set(1280, 720);          // meia-largura 640, meia-altura 360
            Screen.CameraPosition = Vector2.Zero;
            Screen.CameraZoom = 1f;
        }

        [TestMethod]
        public void CameraVision_ContaSoQuemEstaNaTela()
        {
            CameraAtOrigin();
            var scene = new Scene();
            var dentro = new GameObject("A") { Tag = "Inimigo" };
            dentro.Transform.Position = new Vector2(100, 0);
            var fora = new GameObject("B") { Tag = "Inimigo" };
            fora.Transform.Position = new Vector2(5000, 0);
            scene.Add(dentro);
            scene.Add(fora);

            Assert.AreEqual(1, CameraVision.CountOnScreen(scene, "Inimigo"), "só o de dentro do quadro conta");

            fora.Transform.Position = new Vector2(300, 0); // agora ambos na tela
            Assert.AreEqual(2, CameraVision.CountOnScreen(scene, "Inimigo"));
        }

        [TestMethod]
        public void Bateria_SobeComInimigoNaTela_DesceQuandoSai()
        {
            AudioMixer.Reset();
            CameraAtOrigin();

            var scene = new Scene();
            var enemy = new GameObject("Inimigo") { Tag = "Inimigo" };
            enemy.Transform.Position = new Vector2(0, 0); // na tela
            scene.Add(enemy);

            var music = new GameObject("Musica");
            var layered = new LayeredMusic { BaseTrackPath = null, Bus = AudioMixer.Music };
            var drums = new MusicLayer { Tag = "Inimigo", MinCount = 1, OnlyOnScreen = true, FadeTime = 1f, MaxVolume = 1f };
            layered.AddLayer(drums);
            music.AddComponent(layered);
            scene.Add(music);

            scene.StartPlay();
            Assert.AreEqual(0f, drums.Current, 0.001f, "começa silenciosa");

            for (int i = 0; i < 40; i++) scene.Update(Frame(0.033)); // ~1.3s com inimigo na tela
            Assert.IsTrue(drums.Current > 0.9f, $"bateria sobe com inimigo na tela (Current={drums.Current:0.00})");

            enemy.Transform.Position = new Vector2(5000, 0); // sai do quadro
            for (int i = 0; i < 40; i++) scene.Update(Frame(0.033));
            Assert.IsTrue(drums.Current < 0.1f, $"bateria desce quando o inimigo sai (Current={drums.Current:0.00})");
        }

        [TestMethod]
        public void CamadaPorEvento_LigaEDesligaPorMensagem()
        {
            AudioMixer.Reset();
            CameraAtOrigin();

            var scene = new Scene();
            var music = new GameObject("Musica");
            var layered = new LayeredMusic { BaseTrackPath = null };
            var tensao = new MusicLayer { Tag = "", RiseMessage = "chefe", FallMessage = "calmo", FadeTime = 0.5f, MaxVolume = 1f };
            layered.AddLayer(tensao);
            music.AddComponent(layered);
            scene.Add(music);
            scene.StartPlay();

            scene.Send("chefe");
            for (int i = 0; i < 30; i++) scene.Update(Frame(0.033)); // ~1s
            Assert.IsTrue(tensao.Current > 0.9f, $"liga por mensagem (Current={tensao.Current:0.00})");

            scene.Send("calmo");
            for (int i = 0; i < 30; i++) scene.Update(Frame(0.033));
            Assert.IsTrue(tensao.Current < 0.1f, $"desliga por mensagem (Current={tensao.Current:0.00})");
        }
    }
}
