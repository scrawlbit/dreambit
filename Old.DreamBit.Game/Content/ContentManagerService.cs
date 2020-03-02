using System;
using System.Collections.Generic;
using System.Linq;
using DreamBit.Game.Content.Loaders;
using DreamBit.Game.Data;
using DreamBit.Game.Exceptions;
using DreamBit.Game.Reading;
using Microsoft.Xna.Framework.Content;

namespace DreamBit.Game.Content
{
    internal class ContentManagerService : IContentManager, IContentManagerService
    {
        private readonly IDataReader _dataReader;
        private readonly IGameData _gameData;
        private readonly IContentLoader[] _loaders;
        private readonly Dictionary<(Guid? fileId, string assetName), IContent> _contentsLoaded;

        public ContentManagerService(IDataReader dataReader, IGameData gameData)
            : this(dataReader, gameData, new FontLoader(), new ImageLoader(), new SceneLoader())
        {
        }
        internal ContentManagerService(IDataReader dataReader, IGameData gameData, params IContentLoader[] loaders)
        {
            _dataReader = dataReader;
            _gameData = gameData;
            _loaders = loaders;
            _contentsLoaded = new Dictionary<(Guid?, string), IContent>();
        }

        public ContentManager ContentManager { get; set; }
        public bool IsLoaded => ContentManager != null;

        public IContent Load(Type contentType, Guid fileId)
        {
            return Load(contentType, fileId, null);
        }
        public IContent Load(Type contentType, string assetName)
        {
            return Load(contentType, null, assetName);
        }
        public T Load<T>(Guid fileId) where T : IContent
        {
            return (T)Load(typeof(T), fileId);
        }
        public T Load<T>(string assetName) where T : IContent
        {
            return (T)Load(typeof(T), assetName);
        }

        private IContent Load(Type contentType, Guid? fileId, string assetName)
        {
            ValidateLoadParameters(ref fileId, ref assetName);

            if (!_contentsLoaded.TryGetValue((fileId, assetName), out var content))
            {
                var loader = GetLoader(contentType);

                content = loader.Load(fileId, assetName, ContentManager, _dataReader);
                _contentsLoaded.Add((fileId, assetName), content);
            }

            return content;
        }

        private void ValidateLoadParameters(ref Guid? fileId, ref string assetName)
        {
            if (assetName != null)
            {
                var name = assetName;
                fileId = _gameData.ContentPaths.SingleOrDefault(c => c.ContentPath == name)?.FileId;
            }
            else if (fileId != null)
            {
                var id = fileId;
                assetName = _gameData.ContentPaths.SingleOrDefault(c => c.FileId == id)?.ContentPath;
                
                if (assetName == null)
                    throw new ContentNotRegisteredException(fileId.Value);
            }
        }
        private IContentLoader GetLoader(Type contentType)
        {
            var loader = _loaders.SingleOrDefault(l => l.ContentType == contentType);
            if (loader == null)
                throw new ArgumentException($"{contentType.Name} is not a valid content type");

            return loader;
        }
    }
}