namespace DreamBit.Engine.Rendering
{
    /// <summary>
    /// Estado da área de desenho compartilhado com os componentes: tamanho do viewport (em
    /// pixels) e posição atual da câmera no mundo. O host (Player ou preview do editor)
    /// atualiza a cada frame; a UI (âncoras/botões) usa o tamanho e o parallax usa a câmera.
    /// </summary>
    public static class Screen
    {
        public static int Width { get; private set; } = 1280;
        public static int Height { get; private set; } = 720;

        /// <summary>Posição da câmera no mundo (centro), usada pelo parallax.</summary>
        public static Microsoft.Xna.Framework.Vector2 CameraPosition { get; set; }

        public static void Set(int width, int height)
        {
            if (width > 0) Width = width;
            if (height > 0) Height = height;
        }
    }
}
