using Microsoft.Xna.Framework;
using System;

#pragma warning disable 67
namespace DreamBit.Extension.Module.Selection
{
    internal class NoSelectionMode : ISelectionMode
    {
        public event Action NameChanged;
        public event Action AreaChanged;
        public string Name() => null;
        public bool IsVisible() => false;
        public Vector2 Position() => Vector2.Zero;
        public float Rotation() => 0;
        public Vector2 Scale() => Vector2.One;
        public Rectangle Area() => Rectangle.Empty;

        public void Dispose() { }
    }
}
