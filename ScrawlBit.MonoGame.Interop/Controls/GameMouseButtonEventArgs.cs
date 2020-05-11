using System.Windows;
using System.Windows.Input;

namespace ScrawlBit.MonoGame.Interop.Controls
{
    public class GameMouseButtonEventArgs : GameMouseEventArgs
    {
        private readonly MouseButtonEventArgs _args;

        internal GameMouseButtonEventArgs(MouseButtonEventArgs args, IInputElement inputElement) : base(args, inputElement)
        {
            _args = args;
        }

        public MouseButtonState ButtonState => _args.ButtonState;
        public MouseButton ChangedButton => _args.ChangedButton;
        public int ClickCount => _args.ClickCount;
    }
}
