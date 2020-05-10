using DreamBit.Extension.Management;
using DreamBit.Extension.Module.Tools;
using DreamBit.Game.Drawing;
using Scrawlbit.Helpers;
using Scrawlbit.Notification;
using Scrawlbit.Presentation.Commands;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace DreamBit.Extension.Module
{
    public interface IEditorToolBox : INotifyPropertyChanged
    {
        IReadOnlyCollection<IEditorTool> Tools { get; }
        IEditorTool Selected { get; }
        ICommand SelectToolCommand { get; }

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

    internal class EditorToolBox : NotificationObject, IEditorToolBox
    {
        private readonly IEditor _editor;
        private IEditorTool _selected;

        public EditorToolBox(IEditor editor)
        {
            _editor = editor;

            Tools = new IEditorTool[]
            {
                new SelectionTool()
            };

            Selected = Tools.First();
            SelectToolCommand = new DelegateCommand<IEditorTool>(SelectTool, CanSelectTool);
        }

        public IReadOnlyCollection<IEditorTool> Tools { get; }
        public IEditorTool Selected
        {
            get => _selected;
            private set => Set(ref _selected, value);
        }
        public ICommand SelectToolCommand { get; }

        private bool CanSelectTool(IEditorTool tool)
        {
            return _editor.OpenedScene != null && Tools.Contains(tool);
        }
        private void SelectTool(IEditorTool tool)
        {
            Selected = tool;
        }

        public void OnKeyDown(KeyEventArgs args)
        {
            Selected.OnKeyDown(args);
        }
        public void OnKeyUp(KeyEventArgs args)
        {
            Selected.OnKeyUp(args);
        }

        public void OnMouseEnter(MouseEventArgs args)
        {
            Selected.OnMouseEnter(args);
        }
        public void OnMouseMove(MouseEventArgs args)
        {
            Selected.OnMouseMove(args);
        }
        public void OnMouseLeave(MouseEventArgs args)
        {
            Selected.OnMouseLeave(args);
        }
        public void OnMouseDown(MouseButtonEventArgs args)
        {
            Selected.OnMouseDown(args);
        }
        public void OnMouseUp(MouseButtonEventArgs args)
        {
            Selected.OnMouseUp(args);
        }
        public void OnMouseWheel(MouseWheelEventArgs args)
        {
            Selected.OnMouseWheel(args);
        }

        public void Draw(IContentDrawer drawer)
        {
            Selected.Draw(drawer);
        }
    }
}
