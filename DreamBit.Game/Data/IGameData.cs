using System.Collections.Generic;

namespace DreamBit.Game.Data
{
    internal interface IGameData
    {
        string StartScene { get; }
        IReadOnlyList<GameContent> ContentPaths { get; }
        IReadOnlyList<GameScript> ScriptTypes { get; }
    }
}