using DreamBit.Extension.Module.Handlers;
using DreamBit.Game.Drawing;
using Microsoft.Xna.Framework;
using ScrawlBit.MonoGame.Interop.Controls;
using System;
using System.Linq;
using System.Windows.Input;

namespace DreamBit.Extension.Module.Tools
{
    internal abstract class HandlerTool : EditorTool, IEditorTool
    {
        private readonly IEditorHandler[] _handlers;
        private IEditorHandler _current;

        public HandlerTool(params IEditorHandler[] handlers)
        {
            _handlers = handlers;
        }

        public override bool IsMouseOver(Vector2 position) => false;

        public override void OnKeyDown(KeyEventArgs e) => Perform(h => h.OnKeyDown(e));
        public override void OnKeyUp(KeyEventArgs e) => Perform(h => h.OnKeyUp(e));

        public override void OnMouseEnter(GameMouseEventArgs e) => Perform(h => h.OnMouseEnter(e), e.Position);
        public override void OnMouseMove(GameMouseEventArgs e) => Perform(h => h.OnMouseMove(e), e.Position);
        public override void OnMouseLeave(GameMouseEventArgs e) => Perform(h => h.OnMouseLeave(e), e.Position);
        public override void OnMouseDown(GameMouseButtonEventArgs e) => Perform(h => h.OnMouseDown(e), e.Position);
        public override void OnMouseUp(GameMouseButtonEventArgs e) => Perform(h => h.OnMouseUp(e), e.Position);
        public override void OnMouseWheel(GameMouseWheelEventArgs e) => Perform(h => h.OnMouseWheel(e), e.Position);

        public override void Draw(IContentDrawer drawer)
        {
            foreach (var handler in _handlers.OrderBy(h => h.DrawOrder))
                handler.Draw(drawer);
        }

        private void Perform(Action<IEditorHandler> action, Vector2? position = null)
        {
            if (_current != null)
            {
                action(_current);

                if (!_current.IsHandling)
                    _current = null;
            }
            else
            {
                foreach (var handler in _handlers)
                {
                    action(handler);

                    if (handler.IsHandling)
                    {
                        _current = handler;
                        break;
                    }
                }
            }

            IEditorHandler source = _current;

            if (source == null && position != null)
                source = _handlers.FirstOrDefault(h => h.IsMouseOver(position.Value));

            Cursor = source?.Cursor;
        }
    }
}
