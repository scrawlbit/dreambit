using System.Windows;
using System.Windows.Input;

namespace ScrawlBit.MonoGame.Interop.Controls
{
    public class GameMouseWheelEventArgs : GameMouseEventArgs
    {
        private readonly MouseWheelEventArgs _args;

        internal GameMouseWheelEventArgs(MouseWheelEventArgs args, IInputElement inputElement) : base(args, inputElement)
        {
            _args = args;
        }

        public int Delta => _args.Delta;
    }
}
