using System;
using DreamBit.Game.Content;

namespace DreamBit.Game.Tests.Mocks.Content
{
    public class ContentManagerMock : IContentManager
    {
        public IContent ContentToLoad { get; set; }
        public (Type ContentType, Guid? FileId, string AssetName) Request { get; set; }
        public bool IsLoaded { get; set; }

        public IContent Load(Type contentType, Guid fileId)
        {
            Request = (contentType, fileId, null);

            return ContentToLoad;
        }
        public IContent Load(Type contentType, string assetName)
        {
            Request = (contentType, null, assetName);

            return ContentToLoad;
        }
        public T Load<T>(Guid fileId) where T : IContent
        {
            return (T)Load(typeof(T), fileId);
        }
        public T Load<T>(string assetName) where T : IContent
        {
            return (T)Load(typeof(T), assetName);
        }
    }
}