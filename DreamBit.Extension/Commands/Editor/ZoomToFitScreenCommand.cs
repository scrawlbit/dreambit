using DreamBit.Extension.Management;
using DreamBit.Game.Elements;
using Microsoft.Xna.Framework;
using Scrawlbit.Presentation.Commands;
using System.Numerics;
using System.Windows.Input;

namespace DreamBit.Extension.Commands.Editor
{
    internal interface IZoomToFitScreenCommand : ICommand
    {
        bool CanExecute();
        void Execute();
    }

    internal class ZoomToFitScreenCommand : BaseCommand, IZoomToFitScreenCommand
    {
        private readonly IEditor _editor;

        public ZoomToFitScreenCommand(IEditor editor)
        {
            _editor = editor;
        }

        public bool CanExecute()
        {
            return _editor.OpenedScene != null;
        }
        public void Execute()
        {
            Rectangle area = _editor.OpenedScene.Objects.TotalArea();

            if (area == Rectangle.Empty)
                _editor.Camera.Zoom = 1;
            else if (area.Width > area.Height)
                _editor.Camera.Zoom = _editor.Camera.Size.X / area.Width;
            else
                _editor.Camera.Zoom = _editor.Camera.Size.Y / area.Height;

            _editor.Camera.Position = area.Center.ToVector2();
        }
    }
}
