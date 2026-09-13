using System;
using System.Collections.Generic;
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

        // Iluminação 2D (lightmap): disco radial + alvo de render + blend de multiplicação.
        private Texture2D _lightSprite = null!;
        private RenderTarget2D? _lightMap;
        private static readonly BlendState MultiplyBlend = new()
        {
            ColorSourceBlend = Blend.DestinationColor,
            ColorDestinationBlend = Blend.Zero,
            AlphaSourceBlend = Blend.DestinationAlpha,
            AlphaDestinationBlend = Blend.Zero
        };

        public SpriteBatch SpriteBatch => _spriteBatch;
        public Texture2D Pixel => _pixel;

        public Color Background { get; set; } = new(24, 26, 32);
        public Color GridColor { get; set; } = new(44, 48, 58);
        public Color GridAxisColor { get; set; } = new(90, 96, 110);
        public bool ShowGrid { get; set; } = true;
        public bool ShowLedges { get; set; } = true;
        public int GridSize { get; set; } = 32;

        /// <summary>Ledge destacada (selecionada no editor), desenhada em branco.</summary>
        public Ledge? HighlightedLedge { get; set; }

        /// <summary>Mostra um overlay de debug (FPS/contadores) no canto da tela.</summary>
        public bool ShowDebugOverlay { get; set; }

        /// <summary>Linhas do overlay de debug (o host preenche por frame).</summary>
        public System.Collections.Generic.IReadOnlyList<string> DebugLines { get; set; }
            = System.Array.Empty<string>();

        /// <summary>Retângulo de seleção por caixa em andamento (mundo), ou null.</summary>
        public (Vector2 Min, Vector2 Max)? SelectionBox { get; set; }

        public void Initialize(GraphicsDevice device)
        {
            _spriteBatch = new SpriteBatch(device);
            _pixel = new Texture2D(device, 1, 1);
            _pixel.SetData(new[] { Color.White });
            _lightSprite = CreateRadialLight(device, 128);
        }

        /// <summary>Gera um disco de luz radial (branco no centro, some nas bordas) para o lightmap.</summary>
        private static Texture2D CreateRadialLight(GraphicsDevice device, int size)
        {
            var tex = new Texture2D(device, size, size);
            var data = new Color[size * size];
            float r = size / 2f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f - r) / r, dy = (y + 0.5f - r) / r;
                    float d = (float)Math.Sqrt(dx * dx + dy * dy);
                    float v = Math.Clamp(1f - d, 0f, 1f);
                    v = v * v; // queda suave (quadrática)
                    byte b = (byte)(v * 255);
                    data[y * size + x] = new Color(b, b, b, b);
                }
            tex.SetData(data);
            return tex;
        }

        public void Render(Scene scene, Camera2D camera, int width, int height)
        {
            var device = _spriteBatch.GraphicsDevice;
            var view = camera.GetViewMatrix(width, height);

            // Iluminação: monta o lightmap ANTES do mundo (alternar render target depois de
            // desenhar apagaria o backbuffer). Depois multiplica sobre o mundo já desenhado.
            bool lit = BuildLightMap(scene, view, width, height);

            device.Clear(Background);

            _spriteBatch.Begin(transformMatrix: view, samplerState: SamplerState.PointClamp);

            if (ShowGrid)
                DrawGrid(camera, width, height);

            scene.Draw(this);

            if (ShowLedges)
                DrawLedges(scene, camera);

            DrawSelection(scene, camera);

            if (SelectionBox.HasValue)
                DrawSelectionBox(SelectionBox.Value, camera);

            _spriteBatch.End();

            if (lit && _lightMap != null)
            {
                _spriteBatch.Begin(blendState: MultiplyBlend, samplerState: SamplerState.PointClamp);
                _spriteBatch.Draw(_lightMap, new Rectangle(0, 0, width, height), Color.White);
                _spriteBatch.End();
            }

            // Passe de tela (HUD): fora da transformação de câmera. Desenha os objetos
            // marcados como ScreenSpace e o passe legado DrawScreen (TextRenderer.ScreenSpace).
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            scene.DrawScreenSpace(this);
            foreach (var obj in scene.VisibleInDrawOrder())
                foreach (var component in obj.Components)
                    if (component.Enabled)
                        component.DrawScreen(this);

            if (ShowDebugOverlay)
                DrawDebugOverlay();

            _spriteBatch.End();
        }

        /// <summary>Monta o lightmap (ambiente + discos de luz) num render target, em world space.
        /// Retorna false se a cena não tem luzes (aí o mundo é desenhado normal, sem escurecer).
        /// Deve ser chamado ANTES de desenhar o mundo (troca de render target apaga o backbuffer).</summary>
        private bool BuildLightMap(Scene scene, Matrix view, int width, int height)
        {
            var lights = new List<(Vector2 Pos, float Radius, Color Color, float Intensity)>();
            Color ambient = new(40, 44, 60);
            bool hasAmbient = false;

            foreach (var obj in scene.VisibleInDrawOrder())
                foreach (var component in obj.Components)
                {
                    if (!component.Enabled)
                        continue;
                    if (component is Components.Light2D light)
                        lights.Add((obj.Transform.WorldPosition, light.Radius, light.Color, light.Intensity));
                    else if (component is Components.AmbientLight amb && !hasAmbient)
                    {
                        ambient = amb.Color;
                        hasAmbient = true;
                    }
                }

            if (lights.Count == 0)
                return false; // cena sem luzes: renderização normal (totalmente iluminada)

            var device = _spriteBatch.GraphicsDevice;
            if (_lightMap == null || _lightMap.Width != width || _lightMap.Height != height)
            {
                _lightMap?.Dispose();
                _lightMap = new RenderTarget2D(device, width, height);
            }

            device.SetRenderTarget(_lightMap);
            device.Clear(ambient);
            _spriteBatch.Begin(blendState: BlendState.Additive, transformMatrix: view, samplerState: SamplerState.LinearClamp);
            var origin = new Vector2(_lightSprite.Width / 2f, _lightSprite.Height / 2f);
            foreach (var (pos, radius, color, intensity) in lights)
            {
                float scale = radius * 2f / _lightSprite.Width;
                var tint = new Color(
                    (byte)Math.Min(255, color.R * intensity),
                    (byte)Math.Min(255, color.G * intensity),
                    (byte)Math.Min(255, color.B * intensity));
                _spriteBatch.Draw(_lightSprite, pos, null, tint, 0f, origin, scale, SpriteEffects.None, 0f);
            }
            _spriteBatch.End();
            device.SetRenderTarget(null);
            return true;
        }

        private void DrawDebugOverlay()
        {
            const int pad = 6, px = 3, lineH = (PixelFont.GlyphHeight + 2) * 3;
            int maxW = 0;
            foreach (var line in DebugLines)
                maxW = Math.Max(maxW, PixelFont.MeasureWidth(line) * px);

            // Fundo semitransparente atrás do texto.
            _spriteBatch.Draw(_pixel,
                new Rectangle(pad - 3, pad - 3, maxW + 8, DebugLines.Count * lineH + 4),
                new Color(0, 0, 0, 150));

            int y = pad;
            foreach (var line in DebugLines)
            {
                DrawHudText(line, pad, y, px, new Color(120, 240, 140));
                y += lineH;
            }
        }

        /// <summary>Desenha texto com a fonte pixel em coordenadas de tela (para overlays).</summary>
        public void DrawHudText(string text, int originX, int originY, int px, Color color)
        {
            int cursor = originX;
            foreach (char ch in text)
            {
                var glyph = PixelFont.Glyph(ch);
                for (int row = 0; row < PixelFont.GlyphHeight; row++)
                {
                    byte bits = glyph[row];
                    for (int col = 0; col < PixelFont.GlyphWidth; col++)
                        if (((bits >> (PixelFont.GlyphWidth - 1 - col)) & 1) != 0)
                            _spriteBatch.Draw(_pixel,
                                new Rectangle(cursor + col * px, originY + row * px, px, px), color);
                }
                cursor += (PixelFont.GlyphWidth + PixelFont.Spacing) * px;
            }
        }

        private void DrawSelectionBox((Vector2 Min, Vector2 Max) box, Camera2D camera)
        {
            var corners = new[]
            {
                box.Min, new Vector2(box.Max.X, box.Min.Y), box.Max, new Vector2(box.Min.X, box.Max.Y)
            };
            var color = new Color(120, 180, 255);
            float t = 1f / camera.Zoom;

            for (int i = 0; i < 4; i++)
                DrawSegment(corners[i], corners[(i + 1) % 4], color, t);
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

        public void DrawFrame(Matrix world, Vector2 size, Color color, Texture2D texture, Rectangle source,
            SpriteEffects effects = SpriteEffects.None)
        {
            if (source.Width <= 0 || source.Height <= 0)
                return;

            var position = new Vector2(world.M41, world.M42);
            float scaleX = new Vector2(world.M11, world.M12).Length();
            float scaleY = new Vector2(world.M21, world.M22).Length();
            float rotation = (float)Math.Atan2(world.M12, world.M11);

            var origin = new Vector2(source.Width / 2f, source.Height / 2f);
            var drawScale = new Vector2(size.X / source.Width * scaleX, size.Y / source.Height * scaleY);

            _spriteBatch.Draw(texture, position, source, color, rotation, origin, drawScale, effects, 0f);
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

            var line = GridColor;
            var axis = GridAxisColor;

            for (int x = minX; x <= maxX; x += step)
                DrawWorldLine(new Vector2(x, minY), new Vector2(x, maxY), x == 0 ? axis : line, x == 0 ? 2f : 1f, camera);
            for (int y = minY; y <= maxY; y += step)
                DrawWorldLine(new Vector2(minX, y), new Vector2(maxX, y), y == 0 ? axis : line, y == 0 ? 2f : 1f, camera);
        }

        private void DrawSelection(Scene scene, Camera2D camera)
        {
            var selected = new System.Collections.Generic.List<GameObject>();
            foreach (var obj in EnumerateVisible(scene))
                if (obj.IsSelected)
                    selected.Add(obj);

            foreach (var obj in selected)
                DrawOutline(obj.Transform.WorldMatrix, GetVisualSize(obj), Color.White);

            if (selected.Count == 1)
            {
                DrawRotationGizmo(selected[0], camera);
                DrawScaleHandles(selected[0], GetVisualSize(selected[0]), camera);
            }
            else if (selected.Count > 1)
            {
                DrawGroupGizmo(selected, camera);
            }
        }

        private void DrawGroupGizmo(System.Collections.Generic.List<GameObject> selected, Camera2D camera)
        {
            var bounds = GizmoGeometry.GroupBounds(selected);
            var corners = GizmoGeometry.GroupCorners(bounds);
            float thickness = 2f / camera.Zoom;
            var boxColor = new Color(90, 170, 255);

            for (int i = 0; i < 4; i++)
                DrawSegment(corners[i], corners[(i + 1) % 4], boxColor, thickness);

            // handle de rotação (acima do topo)
            var topCenter = new Vector2((bounds.Min.X + bounds.Max.X) / 2f, bounds.Min.Y);
            var handle = GizmoGeometry.GroupRotationHandle(bounds, camera.Zoom);
            var rotColor = new Color(255, 200, 80);
            DrawSegment(topCenter, handle, rotColor, thickness);

            float hs = 10f / camera.Zoom;
            _spriteBatch.Draw(_pixel, new Rectangle((int)(handle.X - hs / 2), (int)(handle.Y - hs / 2), (int)hs, (int)hs), rotColor);

            // handles de escala nos cantos
            var scaleColor = new Color(120, 220, 255);
            foreach (var corner in corners)
                _spriteBatch.Draw(_pixel, new Rectangle((int)(corner.X - hs / 2), (int)(corner.Y - hs / 2), (int)hs, (int)hs), scaleColor);
        }

        private void DrawScaleHandles(GameObject obj, Vector2 size, Camera2D camera)
        {
            float hs = 9f / camera.Zoom;
            var color = new Color(120, 220, 255);

            foreach (var corner in GizmoGeometry.Corners(obj, size))
                _spriteBatch.Draw(_pixel,
                    new Rectangle((int)(corner.X - hs / 2), (int)(corner.Y - hs / 2), (int)hs, (int)hs),
                    color);
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
            _lightSprite?.Dispose();
            _lightMap?.Dispose();
        }
    }
}
