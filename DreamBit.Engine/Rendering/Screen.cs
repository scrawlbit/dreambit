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

        /// <summary>Zoom atual da câmera (1 = 1 px do mundo por px de tela). O host atualiza por frame.</summary>
        public static float CameraZoom { get; set; } = 1f;

        /// <summary>Meias-extensões da view em mundo: metade da largura/altura visíveis a partir do centro.</summary>
        public static Microsoft.Xna.Framework.Vector2 WorldHalfExtents
        {
            get
            {
                float z = CameraZoom <= 0f ? 1f : CameraZoom;
                return new Microsoft.Xna.Framework.Vector2(Width / 2f / z, Height / 2f / z);
            }
        }

        /// <summary>True se o ponto de mundo está dentro do quadro da câmera neste frame.
        /// <paramref name="margin"/> (px de mundo) alarga o teste — use para incluir quem está
        /// prestes a entrar em cena.</summary>
        public static bool IsOnScreen(Microsoft.Xna.Framework.Vector2 worldPoint, float margin = 0f)
        {
            var half = WorldHalfExtents;
            var d = worldPoint - CameraPosition;
            return System.Math.Abs(d.X) <= half.X + margin
                && System.Math.Abs(d.Y) <= half.Y + margin;
        }

        public static void Set(int width, int height)
        {
            if (width > 0) Width = width;
            if (height > 0) Height = height;
        }
    }
}
