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
        public List<AnimatorData> Animators { get; set; } = new();
        public List<TilemapData> Tilemaps { get; set; } = new();
        public List<PlatformerData> Platformers { get; set; } = new();
        public List<GameObjectData> Children { get; set; } = new();
    }

    public sealed class PlatformerData
    {
        public float Gravity { get; set; } = 1400f;
        public float HalfHeight { get; set; } = 24f;
        public float HorizontalSpeed { get; set; }
    }

    public sealed class TilemapData
    {
        public string? TmxPath { get; set; }
    }

    public sealed class AnimatorData
    {
        public string? TexturePath { get; set; }
        public int FrameWidth { get; set; } = 32;
        public int FrameHeight { get; set; } = 32;
        public int FrameCount { get; set; } = 1;
        public float Fps { get; set; } = 8f;
        public bool Loop { get; set; } = true;
        public float Width { get; set; } = 64;
        public float Height { get; set; } = 64;
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
