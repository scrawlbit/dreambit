using System;
using Microsoft.Xna.Framework.Graphics;

namespace DreamBit.Game.Content
{
    public interface IImage : IContent
    {
        Guid? FileId { get; }
        Texture2D Texture { get; }
        int Height { get; }
        int Width { get; }
    }

    internal class Image : IImage
    {
        public Image(Guid? fileId, Texture2D texture)
        {
            FileId = fileId;
            Texture = texture;
        }

        public Guid? FileId { get; }
        public Texture2D Texture { get; }
        public int Height => Texture.Height;
        public int Width => Texture.Width;
    }
}