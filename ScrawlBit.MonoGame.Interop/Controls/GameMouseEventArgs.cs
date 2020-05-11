using Microsoft.Xna.Framework;
using System.Windows;
using System.Windows.Input;

namespace ScrawlBit.MonoGame.Interop.Controls
{
    public class GameMouseEventArgs
    {
        private readonly MouseEventArgs _args;
        private readonly IInputElement _inputElement;
        private Vector2? _position;

        internal GameMouseEventArgs(MouseEventArgs args, IInputElement inputElement)
        {
            _args = args;
            _inputElement = inputElement;
        }

        public Vector2 Position
        {
            get
            {
                if (_position == null)
                {
                    System.Windows.Point point = _args.GetPosition(_inputElement);
                    _position = new Vector2((float)point.X, (float)point.Y);
                }

                return _position.Value;
            }
        }
        public MouseButtonState LeftButton => _args.LeftButton;
        public MouseButtonState MiddleButton => _args.MiddleButton;
        public MouseButtonState RightButton => _args.RightButton;
        public bool Handled
        {
            get => _args.Handled;
            set => _args.Handled = value;
        }
    }
}
