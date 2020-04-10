using DreamBit.Game.Elements;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable 67
namespace DreamBit.Extension.Module.Selection
{
    internal class MultipleSelectionMode : ISelectionMode
    {
        private readonly GameObject[] _gameObjects;

        public MultipleSelectionMode(IEnumerable<GameObject> gameObjects)
        {
            _gameObjects = gameObjects.ToArray();
        }

        public event Action NameChanged;
        public event Action AreaChanged;

        public string Name() => "Selection";
        public bool IsVisible()
        {
            return _gameObjects.All(q => q.IsVisible);
        }
        public Vector2 Position()
        {
            var area = _gameObjects.TotalArea();

            var x = MathHelper.Lerp(area.Left, area.Right, 0.5f);
            var y = MathHelper.Lerp(area.Top, area.Bottom, 0.5f);

            return new Vector2(x, y);
        }
        public float Rotation() => 0;
        public Vector2 Scale() => Vector2.One;
        public Rectangle Area() => _gameObjects.TotalArea();

        private void NotifyAreaChanged()
        {
            AreaChanged?.Invoke();
        }

        public void Dispose()
        {
        }
    }
}
