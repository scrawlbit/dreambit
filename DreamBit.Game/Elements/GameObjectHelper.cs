using Microsoft.Xna.Framework;
using Scrawlbit.MonoGame.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace DreamBit.Game.Elements
{
    public static class GameObjectHelper
    {
        public static Rectangle TotalArea(this IEnumerable<GameObject> gameObjects)
        {
            return RectangleHelper.Union(gameObjects.Select(g => g.TotalArea()));
        }
    }
}
