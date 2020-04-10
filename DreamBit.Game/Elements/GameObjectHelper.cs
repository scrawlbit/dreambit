using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DreamBit.Game.Elements
{
    public static class GameObjectHelper
    {
        public static Rectangle TotalArea(this IEnumerable<GameObject> gameObjects)
        {
            var area = Rectangle.Empty;

            foreach (var gameObject in gameObjects)
            {
                if (area.IsEmpty)
                    area = gameObject.TotalArea();
                else
                    area = Rectangle.Union(area, gameObject.TotalArea());
            }

            return area;
        }
    }
}
