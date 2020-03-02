using System;
using DreamBit.Game.Content;
using DreamBit.Game.Reading;
using Microsoft.Xna.Framework.Content;

namespace DreamBit.Game.Tests.Mocks.Content
{
    internal class ContentLoaderMock : IContentLoader
    {
        public Type ContentType { get; set; }
        public IContent ContentToLoad { get; set; }
        public (Guid? fileId, string AssetName) Request { get; set; }

        public IContent Load(Guid? fileId, string assetName, ContentManager contentManager, IDataReader dataReader)
        {
            Request = (fileId, assetName);

            return ContentToLoad;
        }
    }
}