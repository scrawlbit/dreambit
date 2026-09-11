using System;
using DreamBit.Engine.Notification;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Rendering
{
    /// <summary>
    /// Câmera 2D do editor: pan (posição), zoom e conversão tela↔mundo.
    /// Inspirada na SceneCamera dos projetos Old.*, reescrita para net8.
    /// </summary>
    public sealed class Camera2D : NotificationObject
    {
        private Vector2 _position = Vector2.Zero;
        private float _zoom = 1f;

        /// <summary>Centro da câmera em coordenadas de mundo.</summary>
        public Vector2 Position
        {
            get => _position;
            set => Set(ref _position, value);
        }

        public float Zoom
        {
            get => _zoom;
            set => Set(ref _zoom, MathHelper.Clamp(value, 0.1f, 10f));
        }

        /// <summary>Matriz de view para o SpriteBatch, centralizando a câmera no viewport.</summary>
        public Matrix GetViewMatrix(int viewportWidth, int viewportHeight) =>
            Matrix.CreateTranslation(-_position.X, -_position.Y, 0f) *
            Matrix.CreateScale(_zoom, _zoom, 1f) *
            Matrix.CreateTranslation(viewportWidth / 2f, viewportHeight / 2f, 0f);

        /// <summary>Converte um ponto da tela (pixels do canvas) para coordenadas de mundo.</summary>
        public Vector2 ScreenToWorld(Vector2 screen, int viewportWidth, int viewportHeight)
        {
            var invertible = GetViewMatrix(viewportWidth, viewportHeight);
            return Vector2.Transform(screen, Matrix.Invert(invertible));
        }

        public void Pan(Vector2 screenDelta) => Position -= screenDelta / _zoom;

        public void ZoomAt(Vector2 screenPoint, float factor, int viewportWidth, int viewportHeight)
        {
            var before = ScreenToWorld(screenPoint, viewportWidth, viewportHeight);
            Zoom *= factor;
            var after = ScreenToWorld(screenPoint, viewportWidth, viewportHeight);
            Position += before - after;
        }
    }
}
