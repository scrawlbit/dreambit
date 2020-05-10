using DreamBit.Extension.Management;
using Scrawlbit.Presentation.Commands;
using System.Windows.Input;

namespace DreamBit.Extension.Commands.Editor
{
    internal interface IZoomOutCommand : ICommand
    {
        bool CanExecute();
        void Execute();
    }

    internal class ZoomOutCommand : BaseCommand, IZoomOutCommand
    {
        private readonly IEditor _editor;

        public ZoomOutCommand(IEditor editor)
        {
            _editor = editor;
        }

        public bool CanExecute()
        {
            return _editor.OpenedScene != null && _editor.Camera.Zoom > _editor.Camera.MinZoom;
        }
        public void Execute()
        {
            _editor.Camera.ZoomOut();
        }
    }
}
