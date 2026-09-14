using DreamBit.Engine.Rendering;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>Desenha o contorno de realce de um controle de UI quando ele está com foco.</summary>
    internal static class UiFocusDraw
    {
        private static readonly Color FocusColor = new(255, 210, 90);

        public static void Outline(ISceneDrawing drawing, Rectangle r, bool focused, int thickness = 2)
        {
            if (!focused)
                return;
            var pixel = drawing.Pixel;
            var b = drawing.SpriteBatch;
            b.Draw(pixel, new Rectangle(r.X - thickness, r.Y - thickness, r.Width + thickness * 2, thickness), FocusColor);
            b.Draw(pixel, new Rectangle(r.X - thickness, r.Bottom, r.Width + thickness * 2, thickness), FocusColor);
            b.Draw(pixel, new Rectangle(r.X - thickness, r.Y, thickness, r.Height), FocusColor);
            b.Draw(pixel, new Rectangle(r.Right, r.Y, thickness, r.Height), FocusColor);
        }
    }
}
