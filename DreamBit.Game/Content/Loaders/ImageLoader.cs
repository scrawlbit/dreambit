using System;
using DreamBit.Game.Reading;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DreamBit.Game.Content.Loaders
{
    internal class ImageLoader : IContentLoader
    {
        public Type ContentType => typeof(IImage);

        public IContent Load(Guid? fileId, string assetName, ContentManager contentManager, IDataReader dataReader)
        {
            return new Image(fileId, contentManager.Load<Texture2D>(assetName));
        }
    }
}