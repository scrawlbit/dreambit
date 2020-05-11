﻿using DreamBit.Game.Drawing;
using System.Windows.Input;

namespace DreamBit.Extension.Module.Tools
{
    public interface IEditorTool
    {
        string Icon { get; }
        Key ShortcutKey { get; }

        void OnKeyDown(KeyEventArgs e);
        void OnKeyUp(KeyEventArgs e);

        void OnMouseEnter(MouseEventArgs args);
        void OnMouseMove(MouseEventArgs args);
        void OnMouseLeave(MouseEventArgs args);
        void OnMouseDown(MouseButtonEventArgs args);
        void OnMouseUp(MouseButtonEventArgs args);
        void OnMouseWheel(MouseWheelEventArgs args);

        void Draw(IContentDrawer drawer);
    }

    internal abstract class EditorTool : IEditorTool
    {
        public abstract string Icon { get; }
        public abstract Key ShortcutKey { get; }

        public virtual void OnKeyDown(KeyEventArgs e) { }
        public virtual void OnKeyUp(KeyEventArgs e) { }

        public virtual void OnMouseEnter(MouseEventArgs args) { }
        public virtual void OnMouseMove(MouseEventArgs args) { }
        public virtual void OnMouseLeave(MouseEventArgs args) { }
        public virtual void OnMouseDown(MouseButtonEventArgs args) { }
        public virtual void OnMouseUp(MouseButtonEventArgs args) { }
        public virtual void OnMouseWheel(MouseWheelEventArgs args) { }

        public virtual void Draw(IContentDrawer drawer) { }
    }
}
