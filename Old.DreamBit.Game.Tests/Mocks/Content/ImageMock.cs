using System;
using DreamBit.Game.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DreamBit.Game.Tests.Mocks.Content
{
    public class ImageMock : IImage
    {
        public Guid? FileId { get; set; }
        public Texture2D Texture { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
    }
}