using DreamBit.Pipeline.Files;
using DreamBit.Project;
using Microsoft.Xna.Framework.Graphics;

namespace DreamBit.Game.Content
{
    public class Image : IContent
    {
        public Image(PipelineImage file)
        {
            File = file;
        }

        public PipelineImage File { get; }
        public Texture2D Texture { get; }
        public int Height => Texture.Height;
        public int Width => Texture.Width;
        ProjectFile IContent.File => File;
    }
}