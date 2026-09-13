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
        public string Tag { get; set; } = "";
        public int SortOrder { get; set; }
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
        public List<FollowData> Follows { get; set; } = new();
        public List<TriggerData> Triggers { get; set; } = new();
        public List<ScriptData> Scripts { get; set; } = new();
        public List<BoneData> Bones { get; set; } = new();
        public List<SkeletonData> Skeletons { get; set; } = new();
        public List<MessageListenerData> MessageListeners { get; set; } = new();
        public List<BoxColliderData> BoxColliders { get; set; } = new();
        public List<SceneExitData> SceneExits { get; set; } = new();
        public List<CameraData> Cameras { get; set; } = new();
        public List<TextData> Texts { get; set; } = new();
        public List<TweenData> Tweens { get; set; } = new();
        public List<AnimatorControllerData> AnimatorControllers { get; set; } = new();
        public List<GameObjectData> Children { get; set; } = new();
    }

    public sealed class ScriptData
    {
        public string Source { get; set; } = "";
    }

    public sealed class TriggerData
    {
        public float Width { get; set; } = 32;
        public float Height { get; set; } = 32;
        public byte R { get; set; } = 120;
        public byte G { get; set; } = 230;
        public byte B { get; set; } = 150;
        public string TargetTag { get; set; } = "Player";
        public bool DestroyOnEnter { get; set; } = true;
        public string SendOnEnter { get; set; } = "";
    }

    public sealed class MessageListenerData
    {
        public string Message { get; set; } = "evento";
        public int Reaction { get; set; } // MessageReaction
    }

    public sealed class FollowData
    {
        public System.Guid TargetId { get; set; }
        public float Speed { get; set; } = 4f;
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
        public float HalfWidth { get; set; } = 24f;
        public float HorizontalSpeed { get; set; }
        public bool UseKeyboard { get; set; } = true;
        public float MoveSpeed { get; set; } = 220f;
        public float JumpSpeed { get; set; } = 620f;
    }

    public sealed class BoxColliderData
    {
        public float Width { get; set; } = 64f;
        public float Height { get; set; } = 64f;
        public float OffsetX { get; set; }
        public float OffsetY { get; set; }
    }

    public sealed class SceneExitData
    {
        public float Width { get; set; } = 48f;
        public float Height { get; set; } = 96f;
        public string TargetTag { get; set; } = "Player";
        public string TargetScene { get; set; } = "";
    }

    public sealed class AnimatorControllerData
    {
        public string IdleClip { get; set; } = "idle";
        public string WalkClip { get; set; } = "walk";
        public string JumpClip { get; set; } = "jump";
    }

    public sealed class TweenData
    {
        public int Channel { get; set; } = 1; // TweenChannel.PositionY
        public float From { get; set; }
        public float To { get; set; } = 100f;
        public float Duration { get; set; } = 1f;
        public int Easing { get; set; } = 3; // InOut
        public int Loop { get; set; } = 2; // PingPong
        public bool PlayOnStart { get; set; } = true;
    }

    public sealed class TextData
    {
        public string Text { get; set; } = "TEXTO";
        public byte R { get; set; } = 255;
        public byte G { get; set; } = 255;
        public byte B { get; set; } = 255;
        public int PixelSize { get; set; } = 4;
        public bool ScreenSpace { get; set; }
    }

    public sealed class CameraData
    {
        public string TargetTag { get; set; } = "Player";
        public float DeadzoneWidth { get; set; } = 120f;
        public float DeadzoneHeight { get; set; } = 80f;
        public float SmoothTime { get; set; } = 0.15f;
        public float Zoom { get; set; } = 1f;
        public bool UseBounds { get; set; }
        public float BoundsMinX { get; set; } = -2000f;
        public float BoundsMinY { get; set; } = -2000f;
        public float BoundsMaxX { get; set; } = 2000f;
        public float BoundsMaxY { get; set; } = 2000f;
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
        public List<AnimEventData> Events { get; set; } = new();
    }

    public sealed class AnimEventData
    {
        public int Frame { get; set; }
        public string Name { get; set; } = "";
    }

    public sealed class BoneData
    {
        public float Length { get; set; } = 40f;
        public bool HasRestPose { get; set; }
        public float RestX { get; set; }
        public float RestY { get; set; }
        public float RestRotation { get; set; }
        public float RestScaleX { get; set; } = 1f;
        public float RestScaleY { get; set; } = 1f;
    }

    public sealed class SkeletonData
    {
        public string CurrentClip { get; set; } = "default";
        public List<PoseClipData> Clips { get; set; } = new();

        // Campos legados (formato single-clip) — lidos se Clips estiver vazio.
        public float Duration { get; set; } = 1f;
        public bool Loop { get; set; } = true;
        public int Easing { get; set; }
        public List<PoseKeyframeData> Keyframes { get; set; } = new();
        public List<SkeletonEventData> Events { get; set; } = new();
    }

    public sealed class PoseClipData
    {
        public string Name { get; set; } = "default";
        public float Duration { get; set; } = 1f;
        public bool Loop { get; set; } = true;
        public int Easing { get; set; }
        public List<PoseKeyframeData> Keyframes { get; set; } = new();
        public List<SkeletonEventData> Events { get; set; } = new();
    }

    public sealed class SkeletonEventData
    {
        public float Time { get; set; }
        public string Name { get; set; } = "";
    }

    public sealed class PoseKeyframeData
    {
        public float Time { get; set; }
        public List<BonePoseData> Bones { get; set; } = new();
    }

    public sealed class BonePoseData
    {
        public string Bone { get; set; } = "";
        public float Px { get; set; }
        public float Py { get; set; }
        public float Rot { get; set; }
        public float Sx { get; set; } = 1f;
        public float Sy { get; set; } = 1f;
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
        // Recorte no atlas (opcional). W/H = 0 significa "textura inteira".
        public int SrcX { get; set; }
        public int SrcY { get; set; }
        public int SrcW { get; set; }
        public int SrcH { get; set; }
    }
}
