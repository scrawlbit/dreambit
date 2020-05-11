using DreamBit.Game.Drawing;
using ScrawlBit.MonoGame.Interop.Controls;
using System.Windows.Input;

namespace DreamBit.Extension.Module.Tools
{
    public interface IEditorTool
    {
        string Icon { get; }
        Key ShortcutKey { get; }

        void OnKeyDown(KeyEventArgs e);
        void OnKeyUp(KeyEventArgs e);

        void OnMouseEnter(GameMouseEventArgs args);
        void OnMouseMove(GameMouseEventArgs args);
        void OnMouseLeave(GameMouseEventArgs args);
        void OnMouseDown(GameMouseButtonEventArgs args);
        void OnMouseUp(GameMouseButtonEventArgs args);
        void OnMouseWheel(GameMouseWheelEventArgs args);

        void Draw(IContentDrawer drawer);
    }

    internal abstract class EditorTool : IEditorTool
    {
        public abstract string Icon { get; }
        public abstract Key ShortcutKey { get; }

        public virtual void OnKeyDown(KeyEventArgs e) { }
        public virtual void OnKeyUp(KeyEventArgs e) { }

        public virtual void OnMouseEnter(GameMouseEventArgs args) { }
        public virtual void OnMouseMove(GameMouseEventArgs args) { }
        public virtual void OnMouseLeave(GameMouseEventArgs args) { }
        public virtual void OnMouseDown(GameMouseButtonEventArgs args) { }
        public virtual void OnMouseUp(GameMouseButtonEventArgs args) { }
        public virtual void OnMouseWheel(GameMouseWheelEventArgs args) { }

        public virtual void Draw(IContentDrawer drawer) { }
    }
}
