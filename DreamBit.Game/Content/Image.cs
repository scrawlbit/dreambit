using DreamBit.Pipeline.Files;
using DreamBit.Project;
using Microsoft.Xna.Framework.Graphics;

namespace DreamBit.Game.Content
{
    public interface IImage : IContent
    {
        new IPipelineImage File { get; }
        Texture2D Texture { get; }
        int Height { get; }
        int Width { get; }
    }

    internal class Image : IImage
    {
        private readonly IContentLoader _loader;
        private Texture2D _texture;

        public Image(IPipelineImage file, IContentLoader loader)
        {
            _loader = loader;

            File = file;
        }

        public IPipelineImage File { get; }
        public Texture2D Texture
        {
            get => _texture ?? (_texture = _loader.Load<Texture2D>(File));
        }
        public int Height => Texture.Height;
        public int Width => Texture.Width;
        IProjectFile IContent.File => File;
    }
}