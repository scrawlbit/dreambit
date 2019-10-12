using System.Collections.Generic;
using Newtonsoft.Json;

namespace DreamBit.Game.Data
{
    internal class GameData : IGameData
    {
        public string StartScene { get; set; }
        [JsonProperty("ContentReferences")]
        public IReadOnlyList<GameContent> ContentPaths { get; set; }
        [JsonProperty("ScriptReferences")]
        public IReadOnlyList<GameScript> ScriptTypes { get; set; }
    }
}