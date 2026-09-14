using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using DreamBit.Studio.Editing;
using DreamBit.Studio.ViewModels;
using XnaColor = Microsoft.Xna.Framework.Color;
using XnaMatrix = Microsoft.Xna.Framework.Matrix;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;
using XnaRect = Microsoft.Xna.Framework.Rectangle;

namespace DreamBit.Studio.Avalonia
{
    /// <summary>
    /// Renderiza a cena do DreamBit com o DrawingContext do Avalonia e trata o input
    /// reaproveitando o SceneInputController. Desenho vetorial nativo (cross-platform).
    /// </summary>
    public sealed class SceneView : Control
    {
        private EditorViewModel? _editor;
        private SceneInputController? _input;
        private bool _painting;
        private bool _erasing;

        /// <summary>Ponteiro em coordenadas de tela (pixels do controle) — usado para os botões
        /// de UI durante o play do editor. O MainWindow injeta em Input a cada tick.</summary>
        public XnaVector2 PointerScreen { get; private set; }
        public bool PointerIsDown { get; private set; }

        private readonly IBrush _background = new SolidColorBrush(Color.FromRgb(24, 26, 32));
        private readonly Pen _gridPen = new(new SolidColorBrush(Color.FromRgb(44, 48, 58)), 1);
        private readonly Pen _selectionPen = new(Brushes.White, 2);

        public EditorViewModel? Editor
        {
            get => _editor;
            set
            {
                _editor = value;
                _input = value != null ? new SceneInputController(value) : null;
                InvalidateVisual();
            }
        }

        public override void Render(DrawingContext context)
        {
            var size = Bounds.Size;
            context.FillRectangle(_background, new Rect(size));

            if (_editor == null)
                return;

            int w = Math.Max(1, (int)size.Width);
            int h = Math.Max(1, (int)size.Height);
            Screen.Set(w, h); // âncoras de UI resolvem contra o tamanho do viewport do editor
            Screen.CameraPosition = _editor.Camera.Position; // parallax segue a câmera do editor

            using (context.PushClip(new Rect(size)))
            using (context.PushTransform(ToAvalonia(_editor.Camera.GetViewMatrix(w, h))))
            {
                DrawGrid(context, w, h);

                foreach (var obj in _editor.Scene.VisibleInDrawOrder())
                {
                    if (obj.EffectiveScreenSpace)
                        continue; // objetos HUD desenham no passe de tela, sem câmera

                    var vsize = SceneRenderer.GetVisualSize(obj);
                    var rect = new Rect(-vsize.X / 2, -vsize.Y / 2, vsize.X, vsize.Y);

                    using (context.PushTransform(ToAvalonia(obj.Transform.WorldMatrix)))
                    {
                        var tilemap = obj.Components.OfType<TilemapRenderer>().FirstOrDefault();
                        if (tilemap?.Map != null)
                            DrawTilemap(context, tilemap.Map);
                        else if (!TryDrawTexture(context, obj, rect))
                            context.FillRectangle(new SolidColorBrush(ToColor(FillColor(obj))), rect);

                        if (obj.IsSelected)
                            context.DrawRectangle(null, _selectionPen, rect);
                    }
                }

                DrawLedges(context);
                DrawColliders(context);
                DrawSelectionBox(context);
                DrawGizmos(context);
                DrawWorldTexts(context);
            }

            // Passe de tela (HUD): fora da transformação de câmera.
            DrawScreenSpaceObjects(context);
            DrawScreenTexts(context);
        }

        /// <summary>Objetos marcados como HUD (ScreenSpace): desenhados sem a câmera, com a
        /// posição interpretada como coordenada de tela. Espelha o passe de tela da engine.</summary>
        private void DrawScreenSpaceObjects(DrawingContext context)
        {
            foreach (var obj in _editor!.Scene.VisibleInDrawOrder())
            {
                if (!obj.EffectiveScreenSpace)
                    continue;

                var vsize = SceneRenderer.GetVisualSize(obj);
                var rect = new Rect(-vsize.X / 2, -vsize.Y / 2, vsize.X, vsize.Y);

                var button = obj.Components.OfType<UiButton>().FirstOrDefault();
                if (button != null)
                {
                    // Botão: retângulo em coordenadas de tela, com a cor do estado atual.
                    var br = button.ScreenRect();
                    context.FillRectangle(new SolidColorBrush(ToColor(button.CurrentColor)),
                        new Rect(br.X, br.Y, br.Width, br.Height));
                    if (obj.IsSelected)
                        context.DrawRectangle(null, _selectionPen, new Rect(br.X, br.Y, br.Width, br.Height));
                    continue;
                }

                var slider = obj.Components.OfType<UiSlider>().FirstOrDefault();
                if (slider != null)
                {
                    Fill(context, slider.ScreenRect(), slider.Track);
                    Fill(context, slider.FillRect(), slider.Fill);
                    Fill(context, slider.KnobRect(), slider.Knob);
                    if (obj.IsSelected) Outline(context, slider.ScreenRect());
                    continue;
                }

                var toggle = obj.Components.OfType<UiToggle>().FirstOrDefault();
                if (toggle != null)
                {
                    var tr = toggle.ScreenRect();
                    Fill(context, tr, toggle.Box);
                    if (toggle.IsOn)
                    {
                        int pad = (int)(tr.Width * 0.22);
                        Fill(context, new XnaRect(tr.X + pad, tr.Y + pad, tr.Width - pad * 2, tr.Height - pad * 2), toggle.Check);
                    }
                    if (obj.IsSelected) Outline(context, tr);
                    continue;
                }

                var progress = obj.Components.OfType<UiProgressBar>().FirstOrDefault();
                if (progress != null)
                {
                    Fill(context, progress.ScreenRect(), progress.Track);
                    Fill(context, progress.FillRect(), progress.Fill);
                    if (obj.IsSelected) Outline(context, progress.ScreenRect());
                    continue;
                }

                var scroll = obj.Components.OfType<UiScrollView>().FirstOrDefault();
                if (scroll != null)
                {
                    Fill(context, scroll.ScreenRect(), scroll.Background);
                    if (obj.IsSelected) Outline(context, scroll.ScreenRect());
                    continue;
                }

                var field = obj.Components.OfType<UiTextField>().FirstOrDefault();
                if (field != null)
                {
                    var fr = field.ScreenRect();
                    Fill(context, fr, field.IsFocused ? field.BoxFocused : field.Box);
                    string shown = field.Text.Length > 0 ? field.Text : field.Placeholder;
                    var col = field.Text.Length > 0 ? field.TextColor : new XnaColor(field.TextColor, 0.4f);
                    int ty = fr.Y + (fr.Height - PixelFont.GlyphHeight * field.PixelSize) / 2;
                    DrawPixelText(context, shown, fr.X + 6, ty, field.PixelSize, ToColor(col));
                    if (obj.IsSelected) Outline(context, fr);
                    continue;
                }

                using (context.PushTransform(ToAvalonia(obj.Transform.WorldMatrix)))
                {
                    var tilemap = obj.Components.OfType<TilemapRenderer>().FirstOrDefault();
                    bool hasText = obj.Components.OfType<TextRenderer>().Any();
                    if (tilemap?.Map != null)
                        DrawTilemap(context, tilemap.Map);
                    else if (!TryDrawTexture(context, obj, rect) && !hasText)
                        context.FillRectangle(new SolidColorBrush(ToColor(FillColor(obj))), rect);

                    if (obj.IsSelected)
                        context.DrawRectangle(null, _selectionPen, rect);
                }
            }
        }

        private static void Fill(DrawingContext context, XnaRect r, XnaColor color)
            => context.FillRectangle(new SolidColorBrush(ToColor(color)), new Rect(r.X, r.Y, r.Width, r.Height));

        private void Outline(DrawingContext context, XnaRect r)
            => context.DrawRectangle(null, _selectionPen, new Rect(r.X, r.Y, r.Width, r.Height));

        private void DrawWorldTexts(DrawingContext context)
        {
            foreach (var obj in _editor!.Scene.VisibleInDrawOrder())
            {
                if (obj.EffectiveScreenSpace)
                    continue; // texto de objeto HUD sai no passe de tela
                foreach (var c in obj.Components)
                    if (c is TextRenderer t && !t.ScreenSpace)
                    {
                        var p = obj.Transform.WorldPosition;
                        int w = PixelFont.MeasureWidth(t.DisplayText) * t.PixelSize;
                        int h = PixelFont.GlyphHeight * t.PixelSize;
                        DrawPixelText(context, t.DisplayText, p.X - w / 2f, p.Y - h / 2f, t.PixelSize, ToColor(t.Color));
                    }
            }
        }

        private void DrawScreenTexts(DrawingContext context)
        {
            foreach (var obj in _editor!.Scene.VisibleInDrawOrder())
                foreach (var c in obj.Components)
                    if (c is TextRenderer t && (t.ScreenSpace || obj.EffectiveScreenSpace))
                    {
                        var p = obj.Transform.WorldPosition;
                        // objeto HUD centraliza no ponto (igual à engine); legado usa canto.
                        if (obj.EffectiveScreenSpace && !t.ScreenSpace)
                        {
                            int w = PixelFont.MeasureWidth(t.DisplayText) * t.PixelSize;
                            int h = PixelFont.GlyphHeight * t.PixelSize;
                            DrawPixelText(context, t.DisplayText, p.X - w / 2f, p.Y - h / 2f, t.PixelSize, ToColor(t.Color));
                        }
                        else
                        {
                            DrawPixelText(context, t.DisplayText, p.X, p.Y, t.PixelSize, ToColor(t.Color));
                        }
                    }
        }

        private static void DrawPixelText(DrawingContext context, string text, double ox, double oy, int px, Color color)
        {
            var brush = new SolidColorBrush(color);
            double cursor = 0;
            foreach (char ch in text)
            {
                var glyph = PixelFont.Glyph(ch);
                for (int row = 0; row < PixelFont.GlyphHeight; row++)
                    for (int col = 0; col < PixelFont.GlyphWidth; col++)
                        if (((glyph[row] >> (PixelFont.GlyphWidth - 1 - col)) & 1) != 0)
                            context.FillRectangle(brush, new Rect(ox + cursor + col * px, oy + row * px, px, px));
                cursor += (PixelFont.GlyphWidth + PixelFont.Spacing) * px;
            }
        }

        private static readonly IBrush _handleBrush = new SolidColorBrush(Color.FromRgb(240, 240, 245));
        private static readonly IBrush _rotBrush = new SolidColorBrush(Color.FromRgb(120, 220, 160));
        private readonly Pen _gizmoPen = new(new SolidColorBrush(Color.FromArgb(160, 255, 255, 255)), 1);

        /// <summary>Desenha os gizmos de escala (cantos) e rotação (círculo), 1 objeto ou grupo.
        /// Usa a mesma geometria do SceneInputController, então bate com o hit-test.</summary>
        private void DrawGizmos(DrawingContext context)
        {
            if (_editor == null || !_editor.IsSelectTool || _editor.IsPlaying)
                return;

            var sel = _editor.SelectedObjects;
            if (sel.Count == 0)
                return;

            float zoom = Math.Max(0.0001f, _editor.Camera.Zoom);
            float grab = GizmoGeometry.GrabScreenRadius / zoom;

            if (sel.Count == 1)
            {
                var obj = sel[0];
                foreach (var c in GizmoGeometry.Corners(obj, SceneRenderer.GetVisualSize(obj)))
                    Handle(context, c, grab);

                var rh = GizmoGeometry.RotationHandleWorld(obj, zoom);
                context.DrawLine(_gizmoPen, ToPoint(obj.Transform.WorldPosition), ToPoint(rh));
                Circle(context, rh, grab);
            }
            else
            {
                var bounds = GizmoGeometry.GroupBounds(sel);
                context.DrawRectangle(null, _gizmoPen,
                    new Rect(bounds.Min.X, bounds.Min.Y, bounds.Max.X - bounds.Min.X, bounds.Max.Y - bounds.Min.Y));

                foreach (var c in GizmoGeometry.GroupCorners(bounds))
                    Handle(context, c, grab);

                var rh = GizmoGeometry.GroupRotationHandle(bounds, zoom);
                var topCenter = new XnaVector2((bounds.Min.X + bounds.Max.X) / 2f, bounds.Min.Y);
                context.DrawLine(_gizmoPen, ToPoint(topCenter), ToPoint(rh));
                Circle(context, rh, grab);
            }
        }

        private void Handle(DrawingContext context, XnaVector2 world, float half)
            => context.FillRectangle(_handleBrush, new Rect(world.X - half, world.Y - half, half * 2, half * 2));

        private void Circle(DrawingContext context, XnaVector2 world, float radius)
            => context.DrawEllipse(_rotBrush, _gizmoPen, new Point(world.X, world.Y), radius, radius);

        private static Point ToPoint(XnaVector2 v) => new(v.X, v.Y);

        private static readonly IBrush _colliderFill = new SolidColorBrush(Color.FromArgb(40, 90, 200, 255));
        private readonly Pen _colliderPen = new(new SolidColorBrush(Color.FromArgb(150, 90, 200, 255)), 1);

        private void DrawColliders(DrawingContext context)
        {
            foreach (var obj in Flatten(_editor!.Scene.Objects))
                foreach (var c in obj.Components)
                    if (c is BoxCollider collider)
                    {
                        var (min, max) = collider.WorldBounds();
                        context.DrawRectangle(_colliderFill, _colliderPen,
                            new Rect(min.X, min.Y, max.X - min.X, max.Y - min.Y));
                    }
        }

        private static readonly IBrush _boxFill = new SolidColorBrush(Color.FromArgb(40, 90, 200, 255));

        private void DrawSelectionBox(DrawingContext context)
        {
            if (_input?.BoxSelectWorld is not { } box)
                return;

            var rect = new Rect(box.Min.X, box.Min.Y, box.Max.X - box.Min.X, box.Max.Y - box.Min.Y);
            context.DrawRectangle(_boxFill, new Pen(new SolidColorBrush(Color.FromRgb(90, 200, 255)), 1), rect);
        }

        private void DrawGrid(DrawingContext context, int w, int h)
        {
            var topLeft = _editor!.Camera.ScreenToWorld(XnaVector2.Zero, w, h);
            var bottomRight = _editor.Camera.ScreenToWorld(new XnaVector2(w, h), w, h);

            const int step = 32;
            int minX = (int)Math.Floor(topLeft.X / step) * step;
            int maxX = (int)Math.Ceiling(bottomRight.X / step) * step;
            int minY = (int)Math.Floor(topLeft.Y / step) * step;
            int maxY = (int)Math.Ceiling(bottomRight.Y / step) * step;

            for (int x = minX; x <= maxX; x += step)
                context.DrawLine(_gridPen, new Point(x, minY), new Point(x, maxY));
            for (int y = minY; y <= maxY; y += step)
                context.DrawLine(_gridPen, new Point(minX, y), new Point(maxX, y));
        }

        private void DrawLedges(DrawingContext context)
        {
            foreach (var ledge in _editor!.Scene.Ledges)
            {
                var pen = new Pen(new SolidColorBrush(ledge.OneWay ? Color.FromRgb(120, 220, 160) : Color.FromRgb(90, 200, 255)), 3);
                foreach (var (a, b) in ledge.Segments())
                    context.DrawLine(pen, new Point(a.X, a.Y), new Point(b.X, b.Y));
            }
        }

        // ---- input (reaproveita o SceneInputController) ----

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            Focus();
            var pos = Pos(e);
            var props = e.GetCurrentPoint(this).Properties;
            bool ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);

            PointerScreen = pos;
            if (props.IsLeftButtonPressed)
                PointerIsDown = true;

            // Durante o play, o clique vai para os botões de UI (não para as ferramentas de edição).
            if (_editor != null && _editor.IsPlaying)
            {
                e.Pointer.Capture(this);
                return;
            }

            // Modo carimbo: clique esquerdo posiciona o carimbo atual no ponto (mundo).
            if (props.IsLeftButtonPressed && _editor != null && _editor.StampMode && _editor.HasStamp)
            {
                var world = _editor.Camera.ScreenToWorld(pos, W, H);
                _editor.StampCurrentAt(world);
                e.Pointer.Capture(this);
                InvalidateVisual();
                return;
            }

            // Ferramenta de pincel: pinta (esquerdo) / apaga (direito) tiles no tilemap ativo.
            if (_editor != null && _editor.IsTilemapTool && (props.IsLeftButtonPressed || props.IsRightButtonPressed))
            {
                _painting = true;
                _erasing = props.IsRightButtonPressed;
                _editor.BeginPaintStroke();
                _editor.PaintAt(_editor.Camera.ScreenToWorld(pos, W, H), _erasing);
                e.Pointer.Capture(this);
                InvalidateVisual();
                return;
            }

            // Ferramenta de ledge: desenhar (clique adiciona ponto; direito/duplo finaliza).
            if (_editor != null && _editor.IsLedgeTool)
            {
                var world = _editor.Camera.ScreenToWorld(pos, W, H);
                if (props.IsLeftButtonPressed)
                {
                    if (e.ClickCount >= 2) _editor.FinishLedge();
                    else _editor.AddLedgePoint(world);
                }
                else if (props.IsRightButtonPressed) _editor.FinishLedge();
                else if (props.IsMiddleButtonPressed) _input!.MiddleDown(pos);

                e.Pointer.Capture(this);
                InvalidateVisual();
                return;
            }

            if (props.IsLeftButtonPressed)
                _input!.PrimaryDown(pos, W, H, ctrl);
            else if (props.IsMiddleButtonPressed || props.IsRightButtonPressed)
                _input!.MiddleDown(pos);

            e.Pointer.Capture(this);
            InvalidateVisual();
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            PointerScreen = Pos(e);
            if (_painting && _editor != null)
            {
                _editor.PaintAt(_editor.Camera.ScreenToWorld(Pos(e), W, H), _erasing);
                InvalidateVisual();
                return;
            }
            _input?.Move(Pos(e), W, H);
            InvalidateVisual();
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            PointerScreen = Pos(e);
            PointerIsDown = false;

            if (_editor != null && _editor.IsPlaying)
            {
                e.Pointer.Capture(null);
                return;
            }

            if (_painting && _editor != null)
            {
                _editor.EndPaintStroke();
                _painting = false;
                e.Pointer.Capture(null);
                InvalidateVisual();
                return;
            }
            _input?.Up();
            e.Pointer.Capture(null);
            InvalidateVisual();
        }

        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            _input?.Wheel(Pos(e), e.Delta.Y > 0 ? 1 : -1, W, H);
            InvalidateVisual();
        }

        private int W => Math.Max(1, (int)Bounds.Width);
        private int H => Math.Max(1, (int)Bounds.Height);

        private XnaVector2 Pos(PointerEventArgs e)
        {
            var p = e.GetPosition(this);
            return new XnaVector2((float)p.X, (float)p.Y);
        }

        /// <summary>Desenha um tilemap: cada tile no seu local, recortado do tileset.</summary>
        private static void DrawTilemap(DrawingContext context, DreamBit.Engine.Tilemap.Tilemap map)
        {
            foreach (var layer in map.Layers)
            {
                foreach (var (x, y, gid) in layer.Tiles)
                {
                    var tileset = map.TilesetForGid(gid);
                    if (tileset == null || tileset.Columns <= 0)
                        continue;

                    var bmp = AvaloniaImageCache.Get(tileset.ResolvedImagePath);
                    if (bmp == null)
                        continue;

                    int local = gid - tileset.FirstGid;
                    int col = local % tileset.Columns;
                    int row = local / tileset.Columns;
                    var src = new Rect(col * tileset.TileWidth, row * tileset.TileHeight,
                        tileset.TileWidth, tileset.TileHeight);
                    var dest = new Rect(x * map.TileWidth, y * map.TileHeight, map.TileWidth, map.TileHeight);
                    context.DrawImage(bmp, src, dest);
                }
            }
        }

        /// <summary>Desenha a textura do objeto (sprite com recorte, ou frame do animator) no rect.
        /// Retorna false se não há textura carregável (aí o chamador desenha o retângulo colorido).</summary>
        private static bool TryDrawTexture(DrawingContext context, GameObject obj, Rect rect)
        {
            foreach (var c in obj.Components)
            {
                if (c is SpriteRenderer sprite && !string.IsNullOrEmpty(sprite.TexturePath))
                {
                    var bmp = sprite.ChromaKeyEnabled
                        ? AvaloniaImageCache.GetChromaKeyed(sprite.TexturePath, sprite.ChromaAuto, sprite.ChromaColor, sprite.ChromaTolerance)
                        : AvaloniaImageCache.Get(sprite.TexturePath);
                    if (bmp == null)
                        return false;

                    var src = sprite.SourceRect is { } r
                        ? new Rect(r.X, r.Y, r.Width, r.Height)
                        : new Rect(0, 0, bmp.PixelSize.Width, bmp.PixelSize.Height);
                    context.DrawImage(bmp, src, rect);
                    return true;
                }

                if (c is SpriteAnimator anim && !string.IsNullOrEmpty(anim.TexturePath))
                {
                    var bmp = anim.ChromaKeyEnabled
                        ? AvaloniaImageCache.GetChromaKeyed(anim.TexturePath, anim.ChromaAuto, anim.ChromaColor, anim.ChromaTolerance)
                        : AvaloniaImageCache.Get(anim.TexturePath);
                    if (bmp == null)
                        return false;

                    Rect src;
                    if (anim.Frames.Count > 0)
                    {
                        // Frames explícitos (autodetectados, tamanhos variados).
                        var r = anim.Frames[Math.Clamp(anim.CurrentFrame, 0, anim.Frames.Count - 1)];
                        src = new Rect(r.X, r.Y, r.Width, r.Height);
                    }
                    else
                    {
                        int columns = Math.Max(1, bmp.PixelSize.Width / Math.Max(1, anim.FrameWidth));
                        int frame = Math.Clamp(anim.CurrentFrame, 0, Math.Max(0, anim.FrameCount - 1));
                        int col = frame % columns;
                        int row = frame / columns;
                        src = new Rect(col * anim.FrameWidth, row * anim.FrameHeight, anim.FrameWidth, anim.FrameHeight);
                    }
                    context.DrawImage(bmp, src, rect);
                    return true;
                }
            }

            return false;
        }

        private static XnaColor FillColor(GameObject obj)
        {
            foreach (var c in obj.Components)
                if (c is SpriteRenderer s)
                    return s.Color;
            return new XnaColor(70, 130, 200);
        }

        private static Color ToColor(XnaColor c) => Color.FromArgb(c.A, c.R, c.G, c.B);

        private static Matrix ToAvalonia(XnaMatrix m) => new(m.M11, m.M12, m.M21, m.M22, m.M41, m.M42);

        private static IEnumerable<GameObject> Flatten(IEnumerable<GameObject> objects)
        {
            foreach (var obj in objects)
            {
                yield return obj;
                foreach (var child in Flatten(obj.Children))
                    yield return child;
            }
        }
    }
}
