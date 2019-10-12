using System;
using DreamBit.Game.Reading;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DreamBit.Game.Content.Loaders
{
    internal class FontLoader : IContentLoader
    {
        public Type ContentType => typeof(IFont);

        public IContent Load(Guid? fileId, string assetName, ContentManager contentManager, IDataReader dataReader)
        {
            return new Font(fileId, contentManager.Load<SpriteFont>(assetName));
        }
    }
}