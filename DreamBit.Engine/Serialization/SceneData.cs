using System;
using System.Collections.Generic;

namespace DreamBit.Engine.Serialization
{
    // DTOs de serialização da cena (formato .dbscene em JSON).

    public sealed class SceneData
    {
        public string Name { get; set; } = "Cena";
        public List<GameObjectData> Objects { get; set; } = new();
        public List<LedgeData> Ledges { get; set; } = new();
    }

    public sealed class LedgeData
    {
        public string Name { get; set; } = "Ledge";
        public bool OneWay { get; set; } = true;
        public List<PointData> Points { get; set; } = new();
    }

    public sealed class PointData
    {
        public float X { get; set; }
        public float Y { get; set; }
    }

    public sealed class GameObjectData
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "GameObject";
        public bool IsVisible { get; set; } = true;
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float Rotation { get; set; }
        public float ScaleX { get; set; } = 1f;
        public float ScaleY { get; set; } = 1f;
        public List<SpriteData> Sprites { get; set; } = new();
        public List<RotatorData> Rotators { get; set; } = new();
        public List<GameObjectData> Children { get; set; } = new();
    }

    public sealed class RotatorData
    {
        public float Speed { get; set; } = 1.2f;
    }

    public sealed class SpriteData
    {
        public float Width { get; set; }
        public float Height { get; set; }
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; } = 255;
        public string? TexturePath { get; set; }
    }
}
