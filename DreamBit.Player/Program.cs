namespace DreamBit.Player
{
    internal static class Program
    {
        // Uso: DreamBit.Player [caminho-da-cena.dbscene]
        private static void Main(string[] args)
        {
            string? scenePath = args.Length > 0 ? args[0] : null;
            using var game = new PlayerGame(scenePath);
            game.Run();
        }
    }
}
