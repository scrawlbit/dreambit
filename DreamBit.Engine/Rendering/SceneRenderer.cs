using System;
using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DreamBit.Engine.Rendering
{
    /// <summary>
    /// Desenha uma <see cref="Scene"/> num GraphicsDevice usando a <see cref="Camera2D"/>:
    /// grid do editor, objetos da cena e contorno de seleção. Reúne o papel do
    /// DrawBatchService dos projetos Old.*, reescrito para net8 + MonoGame 3.8.
    /// </summary>
    public sealed class SceneRenderer : ISceneDrawing, IDisposable
    {
        private SpriteBatch _spriteBatch = null!;
        private Texture2D _pixel = null!;

        public SpriteBatch SpriteBatch => _spriteBatch;
        public Texture2D Pixel => _pixel;

        public Color Background { get; set; } = new(24, 26, 32);
        public bool ShowGrid { get; set; } = true;
        public bool ShowLedges { get; set; } = true;
        public int GridSize { get; set; } = 32;

        /// <summary>Ledge destacada (selecionada no editor), desenhada em branco.</summary>
        public Ledge? HighlightedLedge { get; set; }

        public void Initialize(GraphicsDevice device)
        {
            _spriteBatch = new SpriteBatch(device);
            _pixel = new Texture2D(device, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        public void Render(Scene scene, Camera2D camera, int width, int height)
        {
            var device = _spriteBatch.GraphicsDevice;
            device.Clear(Background);

            var view = camera.GetViewMatrix(width, height);
            _spriteBatch.Begin(transformMatrix: view, samplerState: SamplerState.PointClamp);

            if (ShowGrid)
                DrawGrid(camera, width, height);

            scene.Draw(this);

            if (ShowLedges)
                DrawLedges(scene, camera);

            DrawSelection(scene, camera);

            _spriteBatch.End();
        }

        private void DrawLedges(Scene scene, Camera2D camera)
        {
            float thickness = 3f / camera.Zoom;
            float vertex = 5f / camera.Zoom;

            foreach (var ledge in scene.Ledges)
            {
                bool highlighted = ReferenceEquals(ledge, HighlightedLedge);
                var color = highlighted ? Color.White
                    : ledge.OneWay ? new Color(120, 220, 160) : new Color(90, 200, 255);
                float t = highlighted ? thickness * 1.6f : thickness;

                foreach (var (a, b) in ledge.Segments())
                {
                    DrawSegment(a, b, color, t);

                    if (ledge.OneWay)
                        DrawOneWayTicks(a, b, color, camera);
                }

                foreach (var point in ledge.Points)
                    _spriteBatch.Draw(_pixel,
                        new Rectangle((int)(point.X - vertex / 2), (int)(point.Y - vertex / 2), (int)vertex, (int)vertex),
                        Color.White);
            }
        }

        /// <summary>Marcas perpendiculares no lado "de cima" indicando plataforma de mão única.</summary>
        private void DrawOneWayTicks(Vector2 a, Vector2 b, Color color, Camera2D camera)
        {
            var dir = b - a;
            float length = dir.Length();
            if (length < 1f)
                return;

            dir /= length;
            var normal = new Vector2(dir.Y, -dir.X); // aponta para "cima" (lado caminhável)
            float step = 24f;
            float tick = 8f / camera.Zoom;

            for (float d = step / 2; d < length; d += step)
            {
                var p = a + dir * d;
                DrawSegment(p, p + normal * tick, color * 0.6f, 1.5f / camera.Zoom);
            }
        }

        public void DrawQuad(Matrix world, Vector2 size, Color color, Texture2D? texture = null)
        {
            var position = new Vector2(world.M41, world.M42);
            float scaleX = new Vector2(world.M11, world.M12).Length();
            float scaleY = new Vector2(world.M21, world.M22).Length();
            float rotation = (float)Math.Atan2(world.M12, world.M11);

            var tex = texture ?? _pixel;
            var texSize = new Vector2(tex.Width, tex.Height);
            var origin = texSize / 2f;
            var drawScale = new Vector2(size.X / texSize.X * scaleX, size.Y / texSize.Y * scaleY);

            _spriteBatch.Draw(tex, position, null, color, rotation, origin, drawScale, SpriteEffects.None, 0f);
        }

        private void DrawGrid(Camera2D camera, int width, int height)
        {
            var topLeft = camera.ScreenToWorld(Vector2.Zero, width, height);
            var bottomRight = camera.ScreenToWorld(new Vector2(width, height), width, height);

            int step = Math.Max(4, GridSize);
            int minX = (int)Math.Floor(topLeft.X / step) * step;
            int maxX = (int)Math.Ceiling(bottomRight.X / step) * step;
            int minY = (int)Math.Floor(topLeft.Y / step) * step;
            int maxY = (int)Math.Ceiling(bottomRight.Y / step) * step;

            var line = new Color(44, 48, 58);
            var axis = new Color(90, 96, 110);

            for (int x = minX; x <= maxX; x += step)
                DrawWorldLine(new Vector2(x, minY), new Vector2(x, maxY), x == 0 ? axis : line, x == 0 ? 2f : 1f, camera);
            for (int y = minY; y <= maxY; y += step)
                DrawWorldLine(new Vector2(minX, y), new Vector2(maxX, y), y == 0 ? axis : line, y == 0 ? 2f : 1f, camera);
        }

        private void DrawSelection(Scene scene, Camera2D camera)
        {
            foreach (var obj in EnumerateVisible(scene))
            {
                if (!obj.IsSelected)
                    continue;

                var world = obj.Transform.WorldMatrix;
                var size = GetVisualSize(obj);
                DrawOutline(world, size, Color.White);
                DrawRotationGizmo(obj, camera);
            }
        }

        private void DrawRotationGizmo(GameObject obj, Camera2D camera)
        {
            var center = obj.Transform.WorldPosition;
            var handle = GizmoGeometry.RotationHandleWorld(obj, camera.Zoom);
            var color = new Color(255, 200, 80);

            DrawSegment(center, handle, color, 2f / camera.Zoom);

            float hs = 10f / camera.Zoom;
            _spriteBatch.Draw(_pixel,
                new Rectangle((int)(handle.X - hs / 2), (int)(handle.Y - hs / 2), (int)hs, (int)hs),
                color);
        }

        /// <summary>Tamanho visual do objeto: do primeiro SpriteRenderer, ou um padrão.</summary>
        public static Vector2 GetVisualSize(GameObject obj)
        {
            foreach (var component in obj.Components)
                if (component is Components.SpriteRenderer sprite)
                    return sprite.Size;

            return new Vector2(48, 48);
        }

        private void DrawOutline(Matrix world, Vector2 size, Color color)
        {
            var half = size / 2f;
            var a = Vector2.Transform(new Vector2(-half.X, -half.Y), world);
            var b = Vector2.Transform(new Vector2(half.X, -half.Y), world);
            var c = Vector2.Transform(new Vector2(half.X, half.Y), world);
            var d = Vector2.Transform(new Vector2(-half.X, half.Y), world);

            DrawSegment(a, b, color, 2f);
            DrawSegment(b, c, color, 2f);
            DrawSegment(c, d, color, 2f);
            DrawSegment(d, a, color, 2f);
        }

        private void DrawWorldLine(Vector2 from, Vector2 to, Color color, float thickness, Camera2D camera)
        {
            DrawSegment(from, to, color, thickness / camera.Zoom);
        }

        private void DrawSegment(Vector2 from, Vector2 to, Color color, float thickness)
        {
            var edge = to - from;
            float length = edge.Length();
            if (length < 0.0001f)
                return;

            float angle = (float)Math.Atan2(edge.Y, edge.X);
            _spriteBatch.Draw(_pixel, from, null, color, angle, new Vector2(0f, 0.5f),
                new Vector2(length, thickness), SpriteEffects.None, 0f);
        }

        private static System.Collections.Generic.IEnumerable<GameObject> EnumerateVisible(Scene scene)
        {
            foreach (var root in scene.Objects)
                foreach (var obj in Enumerate(root))
                    yield return obj;
        }

        private static System.Collections.Generic.IEnumerable<GameObject> Enumerate(GameObject obj)
        {
            yield return obj;
            foreach (var child in obj.Children)
                foreach (var nested in Enumerate(child))
                    yield return nested;
        }

        public void Dispose()
        {
            _spriteBatch?.Dispose();
            _pixel?.Dispose();
        }
    }
}
