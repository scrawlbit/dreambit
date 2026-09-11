using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DreamBit.Engine.Rendering
{
    /// <summary>
    /// Primitivas de desenho entregues aos componentes durante o render da cena,
    /// já dentro de um SpriteBatch.Begin/End com a matriz da câmera aplicada.
    /// </summary>
    public interface ISceneDrawing
    {
        SpriteBatch SpriteBatch { get; }
        Texture2D Pixel { get; }

        /// <summary>Desenha um quad (textura ou cor sólida) posicionado pela matriz de mundo.</summary>
        void DrawQuad(Matrix world, Vector2 size, Color color, Texture2D? texture = null);
    }
}
