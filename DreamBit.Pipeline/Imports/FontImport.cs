namespace DreamBit.Pipeline.Imports
{
    public interface IFontImport : IContentImport
    {
        bool PremultiplyAlpha { get; set; }
        TextureFormat TextureFormat { get; set; }
    }

    internal class FontImport : ContentImport, IFontImport
    {
        internal FontImport(string path) : base(path, BuildtAction.Build)
        {
            PremultiplyAlpha = true;
            TextureFormat = TextureFormat.Compressed;
        }

        public bool PremultiplyAlpha { get; set; }
        public TextureFormat TextureFormat { get; set; }
    }
}