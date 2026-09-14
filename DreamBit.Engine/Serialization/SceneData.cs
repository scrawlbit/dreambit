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
        public int RenderLayer { get; set; }
        public bool ScreenSpace { get; set; }
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
        public List<UiAnchorData> Anchors { get; set; } = new();
        public List<UiButtonData> Buttons { get; set; } = new();
        public List<ParallaxData> Parallaxes { get; set; } = new();
        public List<TimerComponentData> Timers { get; set; } = new();
        public List<UiLayoutData> Layouts { get; set; } = new();
        public List<UiSliderData> Sliders { get; set; } = new();
        public List<UiToggleData> Toggles { get; set; } = new();
        public List<UiProgressBarData> ProgressBars { get; set; } = new();
        public List<UiTextFieldData> TextFields { get; set; } = new();
        public List<UiNavigatorData> Navigators { get; set; } = new();
        public List<UiScrollViewData> ScrollViews { get; set; } = new();
        public List<RigidbodyData> Rigidbodies { get; set; } = new();
        public List<PropertyAnimatorData> PropertyAnimators { get; set; } = new();
        public List<SpriteAnimatorControllerData> SpriteAnimatorControllers { get; set; } = new();
        public List<NavChaserData> NavChasers { get; set; } = new();
        public List<AudioListenerData> AudioListeners { get; set; } = new();
        public List<Light2DData> Lights { get; set; } = new();
        public List<AmbientLightData> AmbientLights { get; set; } = new();
        public List<TopDownData> TopDowns { get; set; } = new();
        public List<HealthData> Healths { get; set; } = new();
        public List<HurtboxData> Hurtboxes { get; set; } = new();
        public List<HitboxData> Hitboxes { get; set; } = new();
        public List<SpriteFlashData> SpriteFlashes { get; set; } = new();
        public List<JointData> Joints { get; set; } = new();
        public List<ShadowCasterData> ShadowCasters { get; set; } = new();
        public List<YSortData> YSorts { get; set; } = new();
        public List<GameObjectData> Children { get; set; } = new();
    }

    public sealed class YSortData { public float Offset { get; set; } }

    public sealed class JointData
    {
        public int Kind { get; set; }
        public string ConnectedTag { get; set; } = "";
        public float AnchorX { get; set; }
        public float AnchorY { get; set; }
        public bool CollideConnected { get; set; }
        public float Frequency { get; set; }
        public float DampingRatio { get; set; } = 0.5f;
    }

    public sealed class ShadowCasterData
    {
        public float Width { get; set; } = 64f;
        public float Height { get; set; } = 64f;
        public float OffsetX { get; set; }
        public float OffsetY { get; set; }
    }

    public sealed class TopDownData
    {
        public float MoveSpeed { get; set; } = 200f;
        public bool UseKeyboard { get; set; } = true;
        public float HalfWidth { get; set; } = 16f;
        public float HalfHeight { get; set; } = 16f;
    }

    public sealed class HealthData
    {
        public float Max { get; set; } = 100f;
        public float InvulnTime { get; set; } = 0.2f;
        public string SendOnHit { get; set; } = "";
        public string SendOnDeath { get; set; } = "death";
        public bool DestroyOnDeath { get; set; }
    }

    public sealed class HurtboxData
    {
        public float Width { get; set; } = 48f;
        public float Height { get; set; } = 48f;
        public float OffsetX { get; set; }
        public float OffsetY { get; set; }
        public int Team { get; set; }
    }

    public sealed class HitboxData
    {
        public float Width { get; set; } = 60f;
        public float Height { get; set; } = 60f;
        public float OffsetX { get; set; }
        public float OffsetY { get; set; }
        public int Team { get; set; }
        public float Damage { get; set; } = 25f;
        public float ActiveTime { get; set; } = 0.15f;
        public string ActivateOn { get; set; } = "";
    }

    public sealed class SpriteFlashData
    {
        public byte R { get; set; } = 255; public byte G { get; set; } = 255; public byte B { get; set; } = 255;
        public float Duration { get; set; } = 0.1f;
    }

    public sealed class Light2DData
    {
        public float Radius { get; set; } = 200f;
        public float Intensity { get; set; } = 1f;
        public byte R { get; set; } = 255; public byte G { get; set; } = 240; public byte B { get; set; } = 210;
    }

    public sealed class AmbientLightData
    {
        public byte R { get; set; } = 40; public byte G { get; set; } = 44; public byte B { get; set; } = 60;
    }

    public sealed class NavChaserData
    {
        public string TargetTag { get; set; } = "player";
        public float Speed { get; set; } = 120f;
        public float RepathInterval { get; set; } = 0.4f;
        public float ArriveRadius { get; set; } = 10f;
        public bool AllowDiagonal { get; set; } = true;
    }

    public sealed class AudioListenerData { }

    public sealed class ScriptData
    {
        public string Source { get; set; } = "";
        public string SourcePath { get; set; } = "";
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
        public string Bus { get; set; } = "SFX";
        public bool Spatial { get; set; }
        public float MaxDistance { get; set; } = 600f;
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
        public float BlendTime { get; set; } = 0.15f;
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

    public sealed class ParallaxData
    {
        public float FactorX { get; set; } = 0.5f;
        public float FactorY { get; set; } = 1f;
    }

    public sealed class PropertyAnimatorData
    {
        public float Duration { get; set; } = 1f;
        public int Loop { get; set; } = 1; // TweenLoop.Loop
        public bool PlayOnStart { get; set; } = true;
        public List<PropertyTrackData> Tracks { get; set; } = new();
    }

    public sealed class PropertyTrackData
    {
        public int Channel { get; set; }
        public int Easing { get; set; } = 3; // InOut
        /// <summary>Keyframes em pares planos: t0,v0, t1,v1, ...</summary>
        public float[] Keys { get; set; } = System.Array.Empty<float>();
    }

    public sealed class RigidbodyData
    {
        public int Kind { get; set; } = 1;   // RigidbodyKind (1 = Dynamic)
        public int Shape { get; set; }        // ColliderShape (0 = Box)
        public float Width { get; set; } = 48f;
        public float Height { get; set; } = 48f;
        public float Radius { get; set; } = 24f;
        public float Density { get; set; } = 1f;
        public float Friction { get; set; } = 0.3f;
        public float Restitution { get; set; }
        public bool FixedRotation { get; set; }
        public int CollisionCategory { get; set; } = 1;
        public int CollidesWith { get; set; } = -1;
    }

    public sealed class UiLayoutData
    {
        public int Direction { get; set; } // LayoutDirection
        public float Spacing { get; set; } = 12f;
    }

    public sealed class UiSliderData
    {
        public float Value { get; set; } = 1f;
        public float Width { get; set; } = 200f;
        public float Height { get; set; } = 20f;
        public byte TR { get; set; } = 40; public byte TG { get; set; } = 46; public byte TB { get; set; } = 58;
        public byte FR { get; set; } = 90; public byte FG { get; set; } = 170; public byte FB { get; set; } = 255;
        public byte KR { get; set; } = 220; public byte KG { get; set; } = 228; public byte KB { get; set; } = 240;
        public string BusTarget { get; set; } = "";
        public string SendOnChange { get; set; } = "";
    }

    public sealed class UiToggleData
    {
        public bool IsOn { get; set; }
        public float Size { get; set; } = 28f;
        public byte BR { get; set; } = 40; public byte BG { get; set; } = 46; public byte BB { get; set; } = 58;
        public byte CR { get; set; } = 90; public byte CG { get; set; } = 200; public byte CB { get; set; } = 140;
        public string SendOnChange { get; set; } = "";
    }

    public sealed class UiProgressBarData
    {
        public float Value { get; set; } = 1f;
        public float Width { get; set; } = 120f;
        public float Height { get; set; } = 14f;
        public byte TR { get; set; } = 40; public byte TG { get; set; } = 46; public byte TB { get; set; } = 58;
        public byte FR { get; set; } = 90; public byte FG { get; set; } = 200; public byte FB { get; set; } = 140;
    }

    public sealed class UiNavigatorData
    {
        public bool AutoFocusFirst { get; set; } = true;
    }

    public sealed class UiScrollViewData
    {
        public float Width { get; set; } = 240f;
        public float Height { get; set; } = 260f;
        public float Spacing { get; set; } = 8f;
        public float ScrollSpeed { get; set; } = 24f;
        public byte BR { get; set; } = 22; public byte BG { get; set; } = 25; public byte BB { get; set; } = 32;
    }

    public sealed class UiTextFieldData
    {
        public string Text { get; set; } = "";
        public string Placeholder { get; set; } = "…";
        public float Width { get; set; } = 220f;
        public float Height { get; set; } = 34f;
        public int PixelSize { get; set; } = 3;
        public int MaxLength { get; set; } = 32;
        public string SendOnSubmit { get; set; } = "";
    }

    public sealed class TimerComponentData
    {
        public float Duration { get; set; } = 1f;
        public bool Repeat { get; set; }
        public bool AutoStart { get; set; } = true;
        public string SendOnElapsed { get; set; } = "timeout";
        public string StartOn { get; set; } = "";
    }

    public sealed class UiAnchorData
    {
        public int Anchor { get; set; } // AnchorPoint (0 = TopLeft)
        public float OffsetX { get; set; }
        public float OffsetY { get; set; }
    }

    public sealed class UiButtonData
    {
        public float Width { get; set; } = 160f;
        public float Height { get; set; } = 48f;
        public byte NR { get; set; } = 60; public byte NG { get; set; } = 70; public byte NB { get; set; } = 90;
        public byte HR { get; set; } = 90; public byte HG { get; set; } = 110; public byte HB { get; set; } = 150;
        public byte PR { get; set; } = 40; public byte PG { get; set; } = 50; public byte PB { get; set; } = 70;
        public string SendOnClick { get; set; } = "click";
    }

    public sealed class TextData
    {
        public string Text { get; set; } = "TEXTO";
        public string LocKey { get; set; } = "";
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
        public int Priority { get; set; }
        public string ShakeOnMessage { get; set; } = "";
        public float ShakeMessageDuration { get; set; } = 0.3f;
        public float ShakeMessageMagnitude { get; set; } = 12f;
    }

    public sealed class TilemapData
    {
        public string? TmxPath { get; set; }
        public bool Edited { get; set; }
        public bool Solid { get; set; }
        public string SolidLayer { get; set; } = "";
        public int Orientation { get; set; }
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
        /// <summary>Animações de tile: tileId, depois pares (frameTileId, duraçãoMs) planos.</summary>
        public List<TileAnimationData> Animations { get; set; } = new();
    }

    public sealed class TileAnimationData
    {
        public int TileId { get; set; }
        /// <summary>Quadros planos: id0, dur0, id1, dur1, ...</summary>
        public int[] Frames { get; set; } = System.Array.Empty<int>();
    }

    public sealed class TileLayerData
    {
        public string Name { get; set; } = "";
        public bool Visible { get; set; } = true;
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
        /// <summary>Frames explícitos em quádruplas planas: x0,y0,w0,h0, x1,... (vazio = grade uniforme).</summary>
        public int[] Frames { get; set; } = System.Array.Empty<int>();
        // Chroma key (remoção de cor de fundo).
        public bool ChromaKey { get; set; }
        public bool ChromaAuto { get; set; } = true;
        public byte ChromaR { get; set; } = 255; public byte ChromaG { get; set; } public byte ChromaB { get; set; } = 255;
        public int ChromaTolerance { get; set; } = 30;
        public List<AnimEventData> Events { get; set; } = new();
        /// <summary>Espelhamento horizontal inicial.</summary>
        public bool FlipX { get; set; }
        /// <summary>Clipes nomeados (andar/pular/bater); vazio = folha inteira.</summary>
        public List<SpriteClipData> Clips { get; set; } = new();
        /// <summary>Clipe tocado ao iniciar (vazio = nenhum).</summary>
        public string DefaultClip { get; set; } = "";
    }

    public sealed class SpriteClipData
    {
        public string Name { get; set; } = "";
        public int[] Frames { get; set; } = System.Array.Empty<int>();
        public float Fps { get; set; } = 8f;
        public bool Loop { get; set; } = true;
    }

    public sealed class SpriteAnimatorControllerData
    {
        public string IdleClip { get; set; } = "idle";
        public string WalkClip { get; set; } = "walk";
        public string JumpClip { get; set; } = "jump";
        public string AttackClip { get; set; } = "attack";
        public string AttackAction { get; set; } = "Action";
        public bool FlipByVelocity { get; set; } = true;
        public bool ArtFacesRight { get; set; } = true;
        public float BlendTime { get; set; } = 0.1f;
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
        // Chroma key (remoção de cor de fundo).
        public bool ChromaKey { get; set; }
        public bool ChromaAuto { get; set; } = true;
        public byte ChromaR { get; set; } = 255; public byte ChromaG { get; set; } public byte ChromaB { get; set; } = 255;
        public int ChromaTolerance { get; set; } = 30;
    }
}
