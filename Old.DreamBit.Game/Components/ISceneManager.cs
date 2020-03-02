using System;
using DreamBit.Game.Elements;

namespace DreamBit.Game.Components
{
    public interface ISceneManager
    {
        Scene OpenedScene { get; }

        void Load(Guid fileId);
        void Load(string assetName);
    }
}