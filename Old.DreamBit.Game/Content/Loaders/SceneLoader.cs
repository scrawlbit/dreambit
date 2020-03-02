using System;
using DreamBit.Game.Elements;
using DreamBit.Game.Reading;
using Microsoft.Xna.Framework.Content;

namespace DreamBit.Game.Content.Loaders
{
    internal class SceneLoader : IContentLoader
    {
        public Type ContentType => typeof(Scene);

        public IContent Load(Guid? fileId, string assetName, ContentManager contentManager, IDataReader dataReader)
        {
            return dataReader.Load<Scene>(assetName, "scene");
        }
    }
}