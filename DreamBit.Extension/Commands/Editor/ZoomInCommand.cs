using DreamBit.Extension.Management;
using DreamBit.Extension.Module;
using Scrawlbit.Presentation.Commands;
using System.Windows.Input;

namespace DreamBit.Extension.Commands.Editor
{
    internal interface IZoomInCommand : ICommand
    {
        bool CanExecute();
        void Execute();
    }

    internal class ZoomInCommand : BaseCommand, IZoomInCommand
    {
        private readonly IEditor _editor;

        public ZoomInCommand(IEditor editor)
        {
            _editor = editor;
        }

        public bool CanExecute()
        {
            return _editor.OpenedScene != null && _editor.Camera.Zoom < _editor.Camera.MaxZoom;
        }
        public void Execute()
        {
            _editor.Camera.ZoomIn();
        }
    }
}
