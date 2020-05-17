using DreamBit.Game.Drawing;
using Microsoft.Xna.Framework;
using Scrawlbit.Notification;
using ScrawlBit.MonoGame.Interop.Controls;
using System.ComponentModel;
using System.Windows.Input;

namespace DreamBit.Extension.Module.Handlers
{
    public interface IEditorHandler : INotifyPropertyChanged
    {
        Cursor Cursor { get; }
        int DrawOrder { get; set; }
        bool IsHandling { get; }

        bool IsMouseOver(Vector2 position);

        void OnKeyDown(KeyEventArgs e);
        void OnKeyUp(KeyEventArgs e);

        void OnMouseEnter(GameMouseEventArgs e);
        void OnMouseMove(GameMouseEventArgs e);
        void OnMouseLeave(GameMouseEventArgs e);
        void OnMouseDown(GameMouseButtonEventArgs e);
        void OnMouseUp(GameMouseButtonEventArgs e);
        void OnMouseWheel(GameMouseWheelEventArgs e);

        void Draw(IContentDrawer drawer);
    }

    internal abstract class EditorHandler : NotificationObject, IEditorHandler
    {
        private Cursor _cursor;

        public Cursor Cursor
        {
            get => _cursor;
            protected set => Set(ref _cursor, value);
        }
        public bool IsHandling { get; protected set; }
        public int DrawOrder { get; set; }

        public virtual bool IsMouseOver(Vector2 position) => true;

        public virtual void OnKeyDown(KeyEventArgs e) { }
        public virtual void OnKeyUp(KeyEventArgs e) { }

        public virtual void OnMouseEnter(GameMouseEventArgs e) { }
        public virtual void OnMouseMove(GameMouseEventArgs e) { }
        public virtual void OnMouseLeave(GameMouseEventArgs e) { }
        public virtual void OnMouseDown(GameMouseButtonEventArgs e) { }
        public virtual void OnMouseUp(GameMouseButtonEventArgs e) { }
        public virtual void OnMouseWheel(GameMouseWheelEventArgs e) { }

        public virtual void Draw(IContentDrawer drawer) { }
    }
}
