using DreamBit.Pipeline.Files;
using DreamBit.Project;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text;

namespace DreamBit.Game.Content
{
    public interface IFont : IContent
    {
        new IPipelineFont File { get; }
        SpriteFont SpriteFont { get; }

        Vector2 MeasureString(string text);
        Vector2 MeasureString(StringBuilder text);
    }

    internal class Font : IFont
    {
        private readonly IContentLoader _loader;
        private SpriteFont _spriteFont;

        public Font(IPipelineFont file, IContentLoader loader)
        {
            _loader = loader;

            File = file;
        }

        public IPipelineFont File { get; }
        public SpriteFont SpriteFont
        {
            get => _spriteFont ?? (_spriteFont = _loader.Load<SpriteFont>(File));
        }
        IProjectFile IContent.File => File;

        public Vector2 MeasureString(string text)
        {
            return SpriteFont.MeasureString(text);
        }
        public Vector2 MeasureString(StringBuilder text)
        {
            return SpriteFont.MeasureString(text);
        }
    }
}
