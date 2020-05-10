using DreamBit.Extension.Management;
using Scrawlbit.Presentation.Commands;
using System.Windows.Input;

namespace DreamBit.Extension.Commands.Editor
{
    internal interface IZoomToOriginalSizeCommand : ICommand
    {
        bool CanExecute();
        void Execute();
    }

    internal class ZoomToOriginalSizeCommand : BaseCommand, IZoomToOriginalSizeCommand
    {
        private readonly IEditor _editor;

        public ZoomToOriginalSizeCommand(IEditor editor)
        {
            _editor = editor;
        }

        public bool CanExecute()
        {
            return _editor.OpenedScene != null && _editor.Camera.Zoom != 1;
        }
        public void Execute()
        {
            _editor.Camera.Zoom = 1;
        }
    }
}
