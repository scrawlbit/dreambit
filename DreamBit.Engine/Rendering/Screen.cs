namespace DreamBit.Engine.Rendering
{
    /// <summary>
    /// Tamanho atual da área de desenho (viewport), em pixels. O host (Player ou preview do
    /// editor) atualiza a cada frame; componentes de UI (âncoras, botões) leem daqui para se
    /// posicionar em relação às bordas da tela. Equivale ao viewport que Godot/Unity expõem à UI.
    /// </summary>
    public static class Screen
    {
        public static int Width { get; private set; } = 1280;
        public static int Height { get; private set; } = 720;

        public static void Set(int width, int height)
        {
            if (width > 0) Width = width;
            if (height > 0) Height = height;
        }
    }
}
