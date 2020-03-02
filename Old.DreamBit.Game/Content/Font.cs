using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DreamBit.Game.Content
{
    public interface IFont : IContent
    {
        Guid? FileId { get; }
        SpriteFont SpriteFont { get; }

        Vector2 MeasureString(string text);
        Vector2 MeasureString(StringBuilder text);
    }

    internal class Font : IFont
    {
        public Font(Guid? fileId, SpriteFont spriteFont)
        {
            FileId = fileId;
            SpriteFont = spriteFont;
        }

        public Guid? FileId { get; }
        public SpriteFont SpriteFont { get; }

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