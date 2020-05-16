using DreamBit.Extension.Management;
using DreamBit.Extension.Module.Tools;
using DreamBit.Game.Drawing;
using Scrawlbit.Helpers;
using Scrawlbit.Notification;
using Scrawlbit.Presentation.Commands;
using ScrawlBit.MonoGame.Interop.Controls;
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

        void OnMouseEnter(GameMouseEventArgs args);
        void OnMouseMove(GameMouseEventArgs args);
        void OnMouseLeave(GameMouseEventArgs args);
        void OnMouseDown(GameMouseButtonEventArgs args);
        void OnMouseUp(GameMouseButtonEventArgs args);
        void OnMouseWheel(GameMouseWheelEventArgs args);

        void Draw(IContentDrawer drawer);
    }

    internal class EditorToolBox : NotificationObject, IEditorToolBox
    {
        private readonly IEditor _editor;
        private IEditorTool _selected;
        private IEditorTool _lastTool;

        public EditorToolBox(IEditor editor, ICameraTool cameraTool, ISelectionTool selectionTool)
        {
            _editor = editor;

            Tools = new IEditorTool[]
            {
                selectionTool,
                cameraTool
            };

            Selected = selectionTool;
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
            return CanEditScene() && Tools.Contains(tool);
        }
        private void SelectTool(IEditorTool tool)
        {
            Selected = tool;
        }

        public void OnKeyDown(KeyEventArgs args)
        {
            if (!CanEditScene())
                return;

            if (!args.IsRepeat)
            {
                IEditorTool tool = Tools.SingleOrDefault(t => t.ShortcutKey == args.Key);

                if (CanSelectTool(tool) && tool != Selected)
                {
                    if (tool.KeepShortcutPressed)
                        _lastTool = Selected;

                    SelectTool(tool);
                }
            }

            Selected.OnKeyDown(args);
        }
        public void OnKeyUp(KeyEventArgs args)
        {
            if (!CanEditScene())
                return;

            if (args.Key == Selected.ShortcutKey && CanSelectTool(_lastTool))
            {
                SelectTool(_lastTool);
                _lastTool = null;
            }

            Selected.OnKeyUp(args);
        }

        public void OnMouseEnter(GameMouseEventArgs args)
        {
            if (CanEditScene()) Selected.OnMouseEnter(args);
        }
        public void OnMouseMove(GameMouseEventArgs args)
        {
            if (CanEditScene()) Selected.OnMouseMove(args);
        }
        public void OnMouseLeave(GameMouseEventArgs args)
        {
            if (CanEditScene()) Selected.OnMouseLeave(args);
        }
        public void OnMouseDown(GameMouseButtonEventArgs args)
        {
            if (CanEditScene()) Selected.OnMouseDown(args);
        }
        public void OnMouseUp(GameMouseButtonEventArgs args)
        {
            if (CanEditScene()) Selected.OnMouseUp(args);
        }
        public void OnMouseWheel(GameMouseWheelEventArgs args)
        {
            if (CanEditScene()) Selected.OnMouseWheel(args);
        }

        public void Draw(IContentDrawer drawer)
        {
            if (!CanEditScene())
                return;

            _lastTool?.Draw(drawer);
            Selected.Draw(drawer);
        }

        private bool CanEditScene()
        {
            return _editor.OpenedScene != null;
        }
    }
}
