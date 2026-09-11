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
        public List<AudioData> Audios { get; set; } = new();
        public List<ParticleData> Particles { get; set; } = new();
        public List<GameObjectData> Children { get; set; } = new();
    }

    public sealed class ParticleData
    {
        public float EmitRate { get; set; } = 30f;
        public float Lifetime { get; set; } = 1.4f;
        public float Speed { get; set; } = 140f;
        public float Spread { get; set; } = 0.6f;
        public float Size { get; set; } = 8f;
        public float GravityY { get; set; } = 220f;
        public byte R { get; set; } = 255;
        public byte G { get; set; } = 180;
        public byte B { get; set; } = 90;
    }

    public sealed class AudioData
    {
        public string? SoundPath { get; set; }
        public float Volume { get; set; } = 1f;
        public bool PlayOnStart { get; set; } = true;
        public bool Loop { get; set; }
    }

    public sealed class PlatformerData
    {
        public float Gravity { get; set; } = 1400f;
        public float HalfHeight { get; set; } = 24f;
        public float HorizontalSpeed { get; set; }
        public bool UseKeyboard { get; set; } = true;
        public float MoveSpeed { get; set; } = 220f;
        public float JumpSpeed { get; set; } = 620f;
    }

    public sealed class TilemapData
    {
        public string? TmxPath { get; set; }
        public bool Edited { get; set; }
        public int TileWidth { get; set; } = 16;
        public int TileHeight { get; set; } = 16;
        public List<TilesetData> Tilesets { get; set; } = new();
        public List<TileLayerData> Layers { get; set; } = new();
    }

    public sealed class TilesetData
    {
        public int FirstGid { get; set; }
        public string? ImagePath { get; set; }
        public int Columns { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
    }

    public sealed class TileLayerData
    {
        public string Name { get; set; } = "";
        /// <summary>Tiles em triplas planas: x0,y0,gid0, x1,y1,gid1, ...</summary>
        public int[] Tiles { get; set; } = System.Array.Empty<int>();
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
