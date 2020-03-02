using System;
using System.Text;
using DreamBit.Game.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DreamBit.Game.Tests.Mocks.Content
{
    public class FontMock : IFont
    {
        public Guid? FileId { get; set; }
        public SpriteFont SpriteFont { get; set; }

        public Vector2 MeasureString(string text)
        {
            throw new NotImplementedException();
        }
        public Vector2 MeasureString(StringBuilder text)
        {
            throw new NotImplementedException();
        }
    }
}