using System;

namespace DreamBit.Game.Content
{
    public interface IContentManager
    {
        bool IsLoaded { get; }

        IContent Load(Type contentType, Guid fileId);
        IContent Load(Type contentType, string assetName);
        T Load<T>(Guid fileId) where T : IContent;
        T Load<T>(string assetName) where T : IContent;
    }
}