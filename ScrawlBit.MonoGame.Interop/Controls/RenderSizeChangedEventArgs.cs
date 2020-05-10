namespace ScrawlBit.MonoGame.Interop.Controls
{
    public class RenderSizeChangedEventArgs
    {
        internal RenderSizeChangedEventArgs(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int Width { get; }
        public int Height { get; }
    }
}
