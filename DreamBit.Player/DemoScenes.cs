using System;
using System.IO;
using DreamBit.Engine.Audio;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;

namespace DreamBit.Player
{
    /// <summary>
    /// Cenas de demonstração construídas por código (sem depender de arquivos .dbscene),
    /// usadas pelas flags do Player para exercitar recursos da engine.
    /// </summary>
    public static class DemoScenes
    {
        /// <summary>
        /// Demo de música adaptativa em camadas: a câmera fica parada na origem e um inimigo
        /// (tag "Inimigo") entra e sai do quadro repetidamente. O componente
        /// <see cref="LayeredMusic"/> mantém a base tocando e faz o fade da bateria conforme
        /// há inimigo na tela.
        /// </summary>
        public static Scene AdaptiveMusic()
        {
            var scene = new Scene { Name = "Demo — Música Adaptativa" };

            // Fundo (só para dar contexto visual ao quadro da câmera).
            var bg = new GameObject("Fundo") { SortOrder = -10 };
            bg.AddComponent(new SpriteRenderer { Size = new Vector2(1280, 720), Color = new Color(24, 26, 34) });
            scene.Add(bg);

            // Inimigo que cruza o quadro: PositionX de -1400 (fora, à esquerda) a 1400 (fora,
            // à direita) em ping-pong. Com a câmera na origem (meia-largura 640), ele fica
            // "na tela" só na parte central do trajeto.
            var enemy = new GameObject("Inimigo") { Tag = "Inimigo" };
            enemy.Transform.Position = new Vector2(-1400, 0);
            enemy.AddComponent(new SpriteRenderer { Size = new Vector2(140, 140), Color = new Color(220, 70, 70) });
            // Keys em segundos: o último Time precisa casar com a duração para o trajeto
            // ocupar o ciclo inteiro (ida e volta no ping-pong).
            const float cross = 6f;
            var move = new PropertyAnimator { Duration = cross, Loop = TweenLoop.PingPong };
            move.Tracks.Add(new PropertyTrack
            {
                Channel = AnimChannel.PositionX,
                Easing = Scrawlbit.EasingMode.Linear,
                Keys = { new AnimKey(0f, -1400f), new AnimKey(cross, 1400f) }
            });
            enemy.AddComponent(move);
            scene.Add(enemy);

            // Música em camadas: base sempre tocando; bateria sobe quando o inimigo aparece.
            var music = new GameObject("Música");
            var layered = new LayeredMusic
            {
                BaseTrackPath = Audio("base.wav"),
                BaseVolume = 0.8f,
                Bus = AudioMixer.Music
            };
            layered.AddLayer(new MusicLayer
            {
                TrackPath = Audio("drums.wav"),
                Tag = "Inimigo",
                MinCount = 1,
                OnlyOnScreen = true,
                FadeTime = 1.0f,
                MaxVolume = 1.0f
            });
            music.AddComponent(layered);
            scene.Add(music);

            return scene;
        }

        /// <summary>Localiza um .wav da demo procurando a pasta Demos/AdaptiveMusic a partir do
        /// diretório do app e do diretório atual (funciona rodando do bin/).</summary>
        private static string Audio(string file)
        {
            foreach (var root in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
            {
                var dir = new DirectoryInfo(root);
                while (dir != null)
                {
                    var candidate = Path.Combine(dir.FullName, "Demos", "AdaptiveMusic", file);
                    if (File.Exists(candidate))
                        return candidate;
                    dir = dir.Parent;
                }
            }
            return file;
        }
    }
}
