using System;
using System.IO;

namespace DreamBit.Player
{
    internal static class Program
    {
        // Uso: DreamBit.Player [caminho-da-cena.dbscene] [--shot <saida.png>] [--shot-frame N] [--walk]
        // Sem argumento, procura "game.dbscene" ao lado do executável (jogo exportado).
        // --shot captura o backbuffer em PNG após N frames e sai (para gerar previews).
        // --example <nome> roda um exemplo da galeria; --examples-list / --examples-save <pasta>.
        private static void Main(string[] args)
        {
            string? scenePath = null;
            string? shotPath = null;
            string? clip = null;
            string? example = null;
            string? examplesSave = null;
            bool examplesList = false;
            int shotFrame = 110;
            bool walk = false;
            bool grayscale = false;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--shot": shotPath = i + 1 < args.Length ? args[++i] : null; break;
                    case "--shot-frame": if (i + 1 < args.Length && int.TryParse(args[++i], out var n)) shotFrame = n; break;
                    case "--walk": walk = true; break;
                    case "--clip": clip = i + 1 < args.Length ? args[++i] : null; break;
                    case "--grayscale": grayscale = true; break;
                    case "--example": example = i + 1 < args.Length ? args[++i] : null; break;
                    case "--examples-list": examplesList = true; break;
                    case "--examples-save": examplesSave = i + 1 < args.Length ? args[++i] : null; break;
                    default: scenePath ??= args[i]; break;
                }
            }

            if (examplesList)
            {
                foreach (var e in ExampleScenes.All)
                    Console.WriteLine($"{e.Name,-16} {e.Title}");
                return;
            }

            // Salva todos os exemplos como .dbscene (sem abrir janela) para abrir no editor.
            if (examplesSave != null)
            {
                Directory.CreateDirectory(examplesSave);
                foreach (var e in ExampleScenes.All)
                {
                    var path = Path.Combine(examplesSave, e.Name + ".dbscene");
                    DreamBit.Engine.Serialization.SceneSerializer.Save(e.Build(), path);
                    Console.WriteLine($"salvo: {path}");
                }
                return;
            }

            if (scenePath == null && example == null)
            {
                var bundled = Path.Combine(AppContext.BaseDirectory, "game.dbscene");
                if (File.Exists(bundled))
                    scenePath = bundled;
            }

            using var game = new PlayerGame(scenePath, shotPath, shotFrame, walk, clip, grayscale, example);
            game.Run();
        }
    }
}
