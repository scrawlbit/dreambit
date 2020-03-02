using System.Collections.Generic;
using DreamBit.Game.Data;

namespace DreamBit.Game.Tests.Mocks.Data
{
    internal class GameDataMock : IGameData
    {
        public GameDataMock()
        {
            ContentPaths = new List<GameContent>();
            ScriptTypes = new List<GameScript>();
        }

        public string StartScene { get; set; }
        public List<GameContent> ContentPaths { get; }
        public List<GameScript> ScriptTypes { get; }

        IReadOnlyList<GameContent> IGameData.ContentPaths => ContentPaths;
        IReadOnlyList<GameScript> IGameData.ScriptTypes => ScriptTypes;
    }
}