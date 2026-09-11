using System;
using System.IO;

namespace DreamBit.Player
{
    internal static class Program
    {
        // Uso: DreamBit.Player [caminho-da-cena.dbscene]
        // Sem argumento, procura "game.dbscene" ao lado do executável (jogo exportado).
        private static void Main(string[] args)
        {
            string? scenePath = args.Length > 0 ? args[0] : null;

            if (scenePath == null)
            {
                var bundled = Path.Combine(AppContext.BaseDirectory, "game.dbscene");
                if (File.Exists(bundled))
                    scenePath = bundled;
            }

            using var game = new PlayerGame(scenePath);
            game.Run();
        }
    }
}
