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
            int shotFrame = 110;
            bool walk = false;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--shot": shotPath = i + 1 < args.Length ? args[++i] : null; break;
                    case "--shot-frame": if (i + 1 < args.Length && int.TryParse(args[++i], out var n)) shotFrame = n; break;
                    case "--walk": walk = true; break;
                    case "--clip": clip = i + 1 < args.Length ? args[++i] : null; break;
                    default: scenePath ??= args[i]; break;
                }
            }

            if (scenePath == null)
            {
                var bundled = Path.Combine(AppContext.BaseDirectory, "game.dbscene");
                if (File.Exists(bundled))
                    scenePath = bundled;
            }

            using var game = new PlayerGame(scenePath, shotPath, shotFrame, walk, clip);
            game.Run();
        }
    }
}
