using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DreamBit.Studio.Editor
{
    /// <summary>
    /// Mini "editor de cena" de demonstração: desenha um grid quadriculado e um
    /// objeto de jogo (retângulo) que pode ser selecionado e arrastado com o mouse.
    /// Reproduz, em pequena escala, o laço central do editor DreamBit (render + input)
    /// para provar que ele funciona fora do Visual Studio, sobre .NET 8 + MonoGame 3.8.
    /// </summary>
    public sealed class SceneEditorRenderer
    {
        private SpriteBatch? _spriteBatch;
        private Texture2D? _pixel;

        private Vector2 _objectPosition = new(320, 220);
        private readonly Vector2 _objectSize = new(120, 80);
        private float _spin;

        private bool _dragging;
        private Vector2 _dragOffset;
        public bool Selected { get; private set; }

        public void LoadContent(GraphicsDevice device)
        {
            _spriteBatch = new SpriteBatch(device);
            _pixel = new Texture2D(device, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        public void Draw(GraphicsDevice device, double deltaSeconds, int width, int height)
        {
            if (_spriteBatch == null || _pixel == null)
                return;

            _spin += (float)deltaSeconds; // anima algo para provar que o loop está vivo

            device.Clear(new Color(24, 26, 32));
            _spriteBatch.Begin();

            DrawGrid(width, height, 32, new Color(44, 48, 58));
            DrawGrid(width, height, 128, new Color(60, 66, 80));

            // objeto de jogo
            var rect = new Rectangle(
                (int)(_objectPosition.X - _objectSize.X / 2),
                (int)(_objectPosition.Y - _objectSize.Y / 2),
                (int)_objectSize.X, (int)_objectSize.Y);

            var fill = Selected ? new Color(90, 170, 255) : new Color(70, 130, 200);
            _spriteBatch.Draw(_pixel, rect, fill);

            if (Selected)
                DrawOutline(rect, 2, Color.White);

            // pequeno "handle" pulsante para dar vida
            float pulse = 4 + (float)Math.Abs(Math.Sin(_spin * 2)) * 4;
            _spriteBatch.Draw(_pixel,
                new Rectangle((int)_objectPosition.X - (int)pulse / 2, (int)_objectPosition.Y - (int)pulse / 2,
                    (int)pulse, (int)pulse), Color.OrangeRed);

            _spriteBatch.End();
        }

        private void DrawGrid(int width, int height, int step, Color color)
        {
            for (int x = 0; x <= width; x += step)
                _spriteBatch!.Draw(_pixel, new Rectangle(x, 0, 1, height), color);
            for (int y = 0; y <= height; y += step)
                _spriteBatch!.Draw(_pixel, new Rectangle(0, y, width, 1), color);
        }

        private void DrawOutline(Rectangle rect, int thickness, Color color)
        {
            _spriteBatch!.Draw(_pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            _spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
            _spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            _spriteBatch.Draw(_pixel, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
        }

        // ---- Input vindo dos eventos de mouse do WPF (coordenadas da superfície) ----

        public void OnMouseDown(Vector2 position)
        {
            var half = _objectSize / 2f;
            bool hit = Math.Abs(position.X - _objectPosition.X) <= half.X
                    && Math.Abs(position.Y - _objectPosition.Y) <= half.Y;

            Selected = hit;
            if (hit)
            {
                _dragging = true;
                _dragOffset = _objectPosition - position;
            }
        }

        public void OnMouseMove(Vector2 position)
        {
            if (_dragging)
                _objectPosition = position + _dragOffset;
        }

        public void OnMouseUp() => _dragging = false;
    }
}
