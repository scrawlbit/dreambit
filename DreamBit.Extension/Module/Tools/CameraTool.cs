using DreamBit.Extension.Resources;
using Microsoft.Xna.Framework;
using ScrawlBit.MonoGame.Interop.Controls;
using System.Windows.Input;

namespace DreamBit.Extension.Module.Tools
{
    internal interface ICameraTool : IEditorTool { }
    internal class CameraTool : EditorTool, ICameraTool
    {
        private readonly IEditorCamera _camera;
        private Vector2 _initialCameraPosition;
        private Vector2 _initialMousePosition;
        private bool _isMoving;

        public CameraTool(IEditorCamera camera)
        {
            _camera = camera;

            Cursor = CustomCursors.HandOpen;
        }

        public override string Icon => "dragPanel";
        public override Key ShortcutKey => Key.Space;
        public override bool KeepShortcutPressed => true;

        public override void OnMouseDown(GameMouseButtonEventArgs args)
        {
            if (args.ChangedButton == MouseButton.Left)
            {
                _initialCameraPosition = _camera.Position;
                _initialMousePosition = args.Position;
                _isMoving = true;

                Cursor = CustomCursors.HandClose;
            }

            args.Handled = true;
        }
        public override void OnMouseMove(GameMouseEventArgs args)
        {
            if (_isMoving)
                _camera.Position = _initialCameraPosition - (args.Position - _initialMousePosition);

            args.Handled = true;
        }
        public override void OnMouseUp(GameMouseButtonEventArgs args)
        {
            if (_isMoving && args.ChangedButton == MouseButton.Left)
            {
                _isMoving = false;

                Cursor = CustomCursors.HandOpen;
            }

            args.Handled = true;
        }
        public override void OnMouseWheel(GameMouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                _camera.ZoomIn(0.02f);
            else if (e.Delta < 0)
                _camera.ZoomOut(0.02f);

            e.Handled = true;
        }
    }
}
