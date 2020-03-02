using System;
using DreamBit.Game.Reading;
using Microsoft.Xna.Framework.Content;

namespace DreamBit.Game.Content
{
    internal interface IContentLoader
    {
        Type ContentType { get; }

        IContent Load(Guid? fileId, string assetName, ContentManager contentManager, IDataReader dataReader);
    }
}