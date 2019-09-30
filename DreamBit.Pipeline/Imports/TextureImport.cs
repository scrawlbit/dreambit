using Microsoft.Xna.Framework;

namespace DreamBit.Pipeline.Imports
{
    public interface ITextureImport : IContentImport
    {
        Color ColorKeyColor { get; set; }
        bool ColorKeyEnabled { get; set; }
        bool GenerateMipmaps { get; set; }
        bool PremultiplyAlpha { get; set; }
        bool ResizeToPowerOfTwo { get; set; }
        bool MakeSquare { get; set; }
        TextureFormat TextureFormat { get; set; }
    }

    internal class TextureImport : ContentImport, ITextureImport
    {
        public TextureImport(string path) : base(path, BuildtAction.Build)
        {
            ColorKeyColor = new Color(255, 0, 255, 255);
            ColorKeyEnabled = true;
            GenerateMipmaps = false;
            PremultiplyAlpha = true;
            ResizeToPowerOfTwo = false;
            MakeSquare = false;
            TextureFormat = TextureFormat.Color;
        }

        public Color ColorKeyColor { get; set; }
        public bool ColorKeyEnabled { get; set; }
        public bool GenerateMipmaps { get; set; }
        public bool PremultiplyAlpha { get; set; }
        public bool ResizeToPowerOfTwo { get; set; }
        public bool MakeSquare { get; set; }
        public TextureFormat TextureFormat { get; set; }
    }
}