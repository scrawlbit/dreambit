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

namespace DreamBit.Studio.Avalonia
{
    /// <summary>
    /// Renderiza a cena do DreamBit com o DrawingContext do Avalonia e trata o input
    /// reaproveitando o SceneInputController. Equivalente ao MonoGameSurface do WPF,
    /// mas em desenho vetorial nativo (cross-platform).
    /// </summary>
    public sealed class SceneView : Control
    {
        private EditorViewModel? _editor;
        private SceneInputController? _input;

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

            using (context.PushClip(new Rect(size)))
            using (context.PushTransform(ToAvalonia(_editor.Camera.GetViewMatrix(w, h))))
            {
                DrawGrid(context, w, h);

                foreach (var obj in Flatten(_editor.Scene.Objects))
                {
                    if (!obj.IsVisible)
                        continue;

                    var vsize = SceneRenderer.GetVisualSize(obj);
                    var rect = new Rect(-vsize.X / 2, -vsize.Y / 2, vsize.X, vsize.Y);

                    using (context.PushTransform(ToAvalonia(obj.Transform.WorldMatrix)))
                    {
                        if (!TryDrawTexture(context, obj, rect))
                            context.FillRectangle(new SolidColorBrush(ToColor(FillColor(obj))), rect);

                        if (obj.IsSelected)
                            context.DrawRectangle(null, _selectionPen, rect);
                    }
                }

                DrawLedges(context);
                DrawSelectionBox(context);
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

            // Modo carimbo: clique esquerdo posiciona o carimbo atual no ponto (mundo).
            if (props.IsLeftButtonPressed && _editor != null && _editor.StampMode && _editor.HasStamp)
            {
                var world = _editor.Camera.ScreenToWorld(pos, W, H);
                _editor.StampCurrentAt(world);
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
            _input?.Move(Pos(e), W, H);
            InvalidateVisual();
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
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

        /// <summary>Desenha a textura do objeto (sprite com recorte, ou frame do animator) no rect.
        /// Retorna false se não há textura carregável (aí o chamador desenha o retângulo colorido).</summary>
        private static bool TryDrawTexture(DrawingContext context, GameObject obj, Rect rect)
        {
            foreach (var c in obj.Components)
            {
                if (c is SpriteRenderer sprite && !string.IsNullOrEmpty(sprite.TexturePath))
                {
                    var bmp = AvaloniaImageCache.Get(sprite.TexturePath);
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
                    var bmp = AvaloniaImageCache.Get(anim.TexturePath);
                    if (bmp == null)
                        return false;

                    int columns = Math.Max(1, bmp.PixelSize.Width / Math.Max(1, anim.FrameWidth));
                    int frame = Math.Clamp(anim.CurrentFrame, 0, Math.Max(0, anim.FrameCount - 1));
                    int col = frame % columns;
                    int row = frame / columns;
                    var src = new Rect(col * anim.FrameWidth, row * anim.FrameHeight, anim.FrameWidth, anim.FrameHeight);
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
