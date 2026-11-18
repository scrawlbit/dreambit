namespace DreamBit.Engine.Rendering
{
    /// <summary>
    /// Desenho de auxílio (colisores, zonas, portas). Ligado por padrão (editor e Player de
    /// protótipo); um jogo final desliga em <c>GameSettings.ShowDebugShapes</c>.
    /// </summary>
    public static class DebugDraw
    {
        /// <summary>Contorno dos BoxColliders.</summary>
        public static bool Colliders { get; set; } = true;

        /// <summary>Áreas de zonas, portas, interruptores e spawns.</summary>
        public static bool Zones { get; set; } = true;
    }
}
