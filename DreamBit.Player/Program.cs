using System;
using System.IO;

namespace DreamBit.Player
{
    internal static class Program
    {
        // Uso: DreamBit.Player [caminho-da-cena.dbscene] [--shot <saida.png>] [--shot-frame N] [--walk]
        // Sem argumento, procura "game.dbscene" ao lado do executável (jogo exportado).
        // --shot captura o backbuffer em PNG após N frames e sai (para gerar previews).
        private static void Main(string[] args)
        {
            string? scenePath = null;
            string? shotPath = null;
            string? clip = null;
            string? demo = null;
            string? musicSave = null;
            int shotFrame = 110;
            bool walk = false;
            bool grayscale = false;
            float musicLog = 0f;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--shot": shotPath = i + 1 < args.Length ? args[++i] : null; break;
                    case "--shot-frame": if (i + 1 < args.Length && int.TryParse(args[++i], out var n)) shotFrame = n; break;
                    case "--walk": walk = true; break;
                    case "--clip": clip = i + 1 < args.Length ? args[++i] : null; break;
                    case "--grayscale": grayscale = true; break;
                    case "--music-demo": demo = "music"; break;
                    case "--music-log":
                        demo = "music";
                        if (i + 1 < args.Length && float.TryParse(args[i + 1], System.Globalization.CultureInfo.InvariantCulture, out var s))
                        { musicLog = s; i++; }
                        else musicLog = 16f;
                        break;
                    case "--music-save": musicSave = i + 1 < args.Length ? args[++i] : null; break;
                    default: scenePath ??= args[i]; break;
                }
            }

            // Salva a cena da demo em .dbscene (sem abrir janela) para abrir no editor.
            if (musicSave != null)
            {
                DreamBit.Engine.Serialization.SceneSerializer.Save(DemoScenes.AdaptiveMusic(), musicSave);
                Console.WriteLine($"Cena da demo salva em: {musicSave}");
                return;
            }

            if (scenePath == null && demo == null)
            {
                var bundled = Path.Combine(AppContext.BaseDirectory, "game.dbscene");
                if (File.Exists(bundled))
                    scenePath = bundled;
            }

            using var game = new PlayerGame(scenePath, shotPath, shotFrame, walk, clip, grayscale, demo, musicLog);
            game.Run();
        }
    }
}
