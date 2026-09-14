using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Tilemap;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Serialization
{
    /// <summary>
    /// Serialização de cena em JSON (System.Text.Json). Mantém o modelo (Elements/*)
    /// limpo, com a serialização desacoplada.
    /// </summary>
    public static class SceneSerializer
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static void Save(Scene scene, string path)
        {
            var data = ToData(scene);
            File.WriteAllText(path, JsonSerializer.Serialize(data, Options));
        }

        public static Scene Load(string path)
        {
            var data = JsonSerializer.Deserialize<SceneData>(File.ReadAllText(path), Options)
                       ?? new SceneData();
            return FromData(data);
        }

        /// <summary>Serializa a cena para uma string JSON (para snapshots em memória).</summary>
        public static string SaveToString(Scene scene) => JsonSerializer.Serialize(ToData(scene), Options);

        /// <summary>Salva a cena carimbando o GUID de cada asset (referências por ID, à prova de
        /// renomear). <paramref name="projectRoot"/> resolve caminhos relativos.</summary>
        public static void SaveWithAssets(Scene scene, string path, Assets.AssetDatabase db, string projectRoot)
        {
            var data = ToData(scene);
            Assets.SceneAssetResolver.Stamp(data, db, projectRoot);
            File.WriteAllText(path, JsonSerializer.Serialize(data, Options));
        }

        /// <summary>Carrega a cena e, se algum caminho de asset quebrou, reencontra o arquivo pelo
        /// GUID (asset movido/renomeado).</summary>
        public static Scene LoadWithAssets(string path, Assets.AssetDatabase db, string projectRoot)
        {
            var data = JsonSerializer.Deserialize<SceneData>(File.ReadAllText(path), Options) ?? new SceneData();
            Assets.SceneAssetResolver.Resolve(data, db, projectRoot);
            return FromData(data);
        }

        public static Scene LoadFromString(string json)
            => FromData(JsonSerializer.Deserialize<SceneData>(json, Options) ?? new SceneData());

        // ---- Prefabs (um objeto isolado) ----

        public static void SaveObject(GameObject obj, string path)
            => File.WriteAllText(path, JsonSerializer.Serialize(ToData(obj), Options));

        /// <summary>Serializa um objeto (prefab) para string JSON.</summary>
        public static string SaveObjectToString(GameObject obj)
            => JsonSerializer.Serialize(ToData(obj), Options);

        /// <summary>Cria um objeto a partir de JSON de prefab (com novos Ids), pronto para inserir.</summary>
        public static GameObject LoadPrefabFromString(string json)
        {
            var data = JsonSerializer.Deserialize<GameObjectData>(json, Options) ?? new GameObjectData();
            var obj = FromData(data);
            ReassignIds(obj);
            return obj;
        }

        /// <summary>Carrega um prefab (objeto) atribuindo novos Ids, pronto para inserir.</summary>
        public static GameObject LoadPrefab(string path)
        {
            var data = JsonSerializer.Deserialize<GameObjectData>(File.ReadAllText(path), Options) ?? new GameObjectData();
            var obj = FromData(data);
            ReassignIds(obj);
            return obj;
        }

        /// <summary>Retângulos de frame → array plano [x,y,w,h, ...] (para serialização compacta).</summary>
        private static int[] FramesToFlat(System.Collections.Generic.IReadOnlyList<Rectangle> frames)
        {
            var flat = new int[frames.Count * 4];
            for (int i = 0; i < frames.Count; i++)
            {
                flat[i * 4] = frames[i].X;
                flat[i * 4 + 1] = frames[i].Y;
                flat[i * 4 + 2] = frames[i].Width;
                flat[i * 4 + 3] = frames[i].Height;
            }
            return flat;
        }

        private static System.Collections.Generic.IEnumerable<Rectangle> FramesFromFlat(int[] flat)
        {
            if (flat == null)
                yield break;
            for (int i = 0; i + 3 < flat.Length; i += 4)
                yield return new Rectangle(flat[i], flat[i + 1], flat[i + 2], flat[i + 3]);
        }

        /// <summary>Clona um objeto (deep copy via round-trip de dados), com novos Ids —
        /// pronto para inserir na cena. Usado pelo pool de objetos e por spawns em runtime.</summary>
        public static GameObject CloneObject(GameObject source)
        {
            var obj = FromData(ToData(source));
            ReassignIds(obj);
            return obj;
        }

        private static void ReassignIds(GameObject obj)
        {
            obj.Id = System.Guid.NewGuid();
            foreach (var child in obj.Children)
                ReassignIds(child);
        }

        // ---- modelo -> dados ----

        private static SceneData ToData(Scene scene)
        {
            var data = new SceneData { Name = scene.Name };
            foreach (var obj in scene.Objects)
                data.Objects.Add(ToData(obj));
            foreach (var ledge in scene.Ledges)
                data.Ledges.Add(ToData(ledge));
            return data;
        }

        private static LedgeData ToData(Ledge ledge)
        {
            var data = new LedgeData { Name = ledge.Name, OneWay = ledge.OneWay };
            foreach (var point in ledge.Points)
                data.Points.Add(new PointData { X = point.X, Y = point.Y });
            return data;
        }

        private static PoseClipData ToData(DreamBit.Engine.Animation.PoseClip clip)
        {
            return new PoseClipData
            {
                Name = clip.Name,
                Duration = clip.Duration,
                Loop = clip.Loop,
                Easing = (int)clip.Easing,
                Events = clip.Events.Select(ev => new SkeletonEventData { Time = ev.Time, Name = ev.Name }).ToList(),
                Keyframes = clip.Keyframes.Select(k => new PoseKeyframeData
                {
                    Time = k.Time,
                    Bones = k.Bones.Select(b => new BonePoseData
                    {
                        Bone = b.Key,
                        Px = b.Value.Position.X, Py = b.Value.Position.Y,
                        Rot = b.Value.Rotation,
                        Sx = b.Value.Scale.X, Sy = b.Value.Scale.Y
                    }).ToList()
                }).ToList()
            };
        }

        private static TilemapData ToData(TilemapRenderer tilemap)
        {
            var data = new TilemapData { TmxPath = tilemap.TmxPath, Edited = tilemap.Edited, Solid = tilemap.Solid, SolidLayer = tilemap.SolidLayer, Orientation = (int)tilemap.Orientation };
            var map = tilemap.Map;

            // Serializa o mapa inline quando pintado (ou quando não veio de .tmx).
            bool inline = map != null && (tilemap.Edited || string.IsNullOrEmpty(tilemap.TmxPath));
            if (inline && map != null)
            {
                data.TileWidth = map.TileWidth;
                data.TileHeight = map.TileHeight;

                foreach (var ts in map.Tilesets)
                {
                    var tsd = new TilesetData
                    {
                        FirstGid = ts.FirstGid,
                        ImagePath = ts.ResolvedImagePath,
                        Columns = ts.Columns,
                        TileWidth = ts.TileWidth,
                        TileHeight = ts.TileHeight
                    };
                    foreach (var (tileId, frames) in ts.Animations)
                    {
                        var flat = new List<int>();
                        foreach (var f in frames) { flat.Add(f.TileId); flat.Add(f.DurationMs); }
                        tsd.Animations.Add(new TileAnimationData { TileId = tileId, Frames = flat.ToArray() });
                    }
                    data.Tilesets.Add(tsd);
                }

                foreach (var layer in map.Layers)
                {
                    var flat = new List<int>();
                    foreach (var (x, y, gid) in layer.Tiles)
                    {
                        flat.Add(x);
                        flat.Add(y);
                        flat.Add(gid);
                    }
                    data.Layers.Add(new TileLayerData { Name = layer.Name, Visible = layer.Visible, Tiles = flat.ToArray() });
                }
            }

            return data;
        }

        private static TilemapRenderer FromData(TilemapData data)
        {
            var tilemap = new TilemapRenderer { TmxPath = data.TmxPath, Edited = data.Edited, Solid = data.Solid, SolidLayer = data.SolidLayer, Orientation = (TileOrientation)data.Orientation };

            if (data.Tilesets.Count > 0 || data.Layers.Count > 0)
            {
                var map = new Tilemap.Tilemap { TileWidth = data.TileWidth, TileHeight = data.TileHeight };

                foreach (var ts in data.Tilesets)
                {
                    var tileset = new Tileset
                    {
                        FirstGid = ts.FirstGid,
                        ResolvedImagePath = ts.ImagePath,
                        Columns = ts.Columns,
                        TileWidth = ts.TileWidth,
                        TileHeight = ts.TileHeight
                    };
                    foreach (var an in ts.Animations)
                    {
                        var frames = new System.Collections.Generic.List<TileAnimationFrame>();
                        for (int i = 0; i + 1 < an.Frames.Length; i += 2)
                            frames.Add(new TileAnimationFrame(an.Frames[i], an.Frames[i + 1]));
                        if (frames.Count > 0)
                            tileset.Animations[an.TileId] = frames;
                    }
                    map.Tilesets.Add(tileset);
                }

                foreach (var ld in data.Layers)
                {
                    var layer = new TileLayer { Name = ld.Name, Visible = ld.Visible };
                    for (int i = 0; i + 2 < ld.Tiles.Length; i += 3)
                        layer.SetTile(ld.Tiles[i], ld.Tiles[i + 1], ld.Tiles[i + 2]);
                    map.Layers.Add(layer);
                }

                tilemap.Map = map;
            }

            return tilemap;
        }

        private static GameObjectData ToData(GameObject obj)
        {
            var t = obj.Transform;
            var data = new GameObjectData
            {
                Id = obj.Id,
                Name = obj.Name,
                Tag = obj.Tag,
                SortOrder = obj.SortOrder,
                RenderLayer = obj.RenderLayer,
                ScreenSpace = obj.ScreenSpace,
                IsVisible = obj.IsVisible,
                Locked = obj.Locked,
                PositionX = t.Position.X,
                PositionY = t.Position.Y,
                Rotation = t.Rotation,
                ScaleX = t.Scale.X,
                ScaleY = t.Scale.Y
            };

            foreach (var component in obj.Components)
            {
                if (component is SpriteRenderer sprite)
                    data.Sprites.Add(new SpriteData
                    {
                        Width = sprite.Size.X,
                        Height = sprite.Size.Y,
                        R = sprite.Color.R,
                        G = sprite.Color.G,
                        B = sprite.Color.B,
                        A = sprite.Color.A,
                        TexturePath = sprite.TexturePath,
                        SrcX = sprite.SourceRect?.X ?? 0,
                        SrcY = sprite.SourceRect?.Y ?? 0,
                        SrcW = sprite.SourceRect?.Width ?? 0,
                        SrcH = sprite.SourceRect?.Height ?? 0,
                        ChromaKey = sprite.ChromaKeyEnabled,
                        ChromaAuto = sprite.ChromaAuto,
                        ChromaR = sprite.ChromaColor.R, ChromaG = sprite.ChromaColor.G, ChromaB = sprite.ChromaColor.B,
                        ChromaTolerance = sprite.ChromaTolerance
                    });
                else if (component is RotatorBehavior rotator)
                    data.Rotators.Add(new RotatorData { Speed = rotator.Speed });
                else if (component is SpriteAnimator animator)
                    data.Animators.Add(new AnimatorData
                    {
                        TexturePath = animator.TexturePath,
                        FrameWidth = animator.FrameWidth,
                        FrameHeight = animator.FrameHeight,
                        FrameCount = animator.FrameCount,
                        Fps = animator.Fps,
                        Loop = animator.Loop,
                        Width = animator.Size.X,
                        Height = animator.Size.Y,
                        Frames = FramesToFlat(animator.Frames),
                        ChromaKey = animator.ChromaKeyEnabled,
                        ChromaAuto = animator.ChromaAuto,
                        ChromaR = animator.ChromaColor.R, ChromaG = animator.ChromaColor.G, ChromaB = animator.ChromaColor.B,
                        ChromaTolerance = animator.ChromaTolerance,
                        FlipX = animator.FlipX,
                        DefaultClip = animator.CurrentClip ?? string.Empty,
                        Clips = animator.Clips
                            .Select(c => new SpriteClipData { Name = c.Name, Frames = c.Frames, Fps = c.Fps, Loop = c.Loop })
                            .ToList(),
                        Events = animator.Events
                            .Select(ev => new AnimEventData { Frame = ev.Frame, Name = ev.Name })
                            .ToList()
                    });
                else if (component is TilemapRenderer tilemap)
                    data.Tilemaps.Add(ToData(tilemap));
                else if (component is AudioSource audio)
                    data.Audios.Add(new AudioData
                    {
                        SoundPath = audio.SoundPath,
                        Volume = audio.Volume,
                        PlayOnStart = audio.PlayOnStart,
                        Loop = audio.Loop,
                        Bus = audio.Bus,
                        Spatial = audio.Spatial,
                        MaxDistance = audio.MaxDistance
                    });
                else if (component is ParticleEmitter particles)
                    data.Particles.Add(new ParticleData
                    {
                        EmitRate = particles.EmitRate,
                        Lifetime = particles.Lifetime,
                        Speed = particles.Speed,
                        Spread = particles.Spread,
                        Size = particles.Size,
                        GravityY = particles.GravityY,
                        R = particles.Color.R,
                        G = particles.Color.G,
                        B = particles.Color.B,
                        EndR = particles.EndColor.R, EndG = particles.EndColor.G, EndB = particles.EndColor.B,
                        EndSize = particles.EndSize, BurstCount = particles.BurstCount, EmitOnStart = particles.EmitOnStart
                    });
                else if (component is FollowTarget follow)
                    data.Follows.Add(new FollowData { TargetId = follow.TargetId, Speed = follow.Speed });
                else if (component is TriggerZone trigger)
                    data.Triggers.Add(new TriggerData
                    {
                        Width = trigger.Size.X,
                        Height = trigger.Size.Y,
                        R = trigger.Color.R,
                        G = trigger.Color.G,
                        B = trigger.Color.B,
                        TargetTag = trigger.TargetTag,
                        DestroyOnEnter = trigger.DestroyOnEnter,
                        SendOnEnter = trigger.SendOnEnter
                    });
                else if (component is MessageListener listener)
                    data.MessageListeners.Add(new MessageListenerData
                    {
                        Message = listener.Message,
                        Reaction = (int)listener.Reaction
                    });
                else if (component is ScriptComponent script)
                    data.Scripts.Add(new ScriptData { Source = script.Source, SourcePath = script.SourcePath });
                else if (component is Bone bone)
                    data.Bones.Add(new BoneData
                    {
                        Length = bone.Length,
                        HasRestPose = bone.HasRestPose,
                        RestX = bone.RestPosition.X,
                        RestY = bone.RestPosition.Y,
                        RestRotation = bone.RestRotation,
                        RestScaleX = bone.RestScale.X,
                        RestScaleY = bone.RestScale.Y
                    });
                else if (component is SkeletonAnimator skeleton)
                    data.Skeletons.Add(new SkeletonData
                    {
                        CurrentClip = skeleton.CurrentClipName,
                        Clips = skeleton.Clips.Select(ToData).ToList()
                    });
                else if (component is PlatformerController platformer)
                    data.Platformers.Add(new PlatformerData
                    {
                        Gravity = platformer.Gravity,
                        HalfHeight = platformer.HalfHeight,
                        HalfWidth = platformer.HalfWidth,
                        HorizontalSpeed = platformer.HorizontalSpeed,
                        UseKeyboard = platformer.UseKeyboard,
                        MoveSpeed = platformer.MoveSpeed,
                        JumpSpeed = platformer.JumpSpeed
                    });
                else if (component is BoxCollider box)
                    data.BoxColliders.Add(new BoxColliderData
                    {
                        Width = box.Size.X,
                        Height = box.Size.Y,
                        OffsetX = box.Offset.X,
                        OffsetY = box.Offset.Y
                    });
                else if (component is SceneExit exit)
                    data.SceneExits.Add(new SceneExitData
                    {
                        Width = exit.Size.X,
                        Height = exit.Size.Y,
                        TargetTag = exit.TargetTag,
                        TargetScene = exit.TargetScene
                    });
                else if (component is AnimatorController animCtrl)
                    data.AnimatorControllers.Add(new AnimatorControllerData
                    {
                        IdleClip = animCtrl.IdleClip, WalkClip = animCtrl.WalkClip, JumpClip = animCtrl.JumpClip, BlendTime = animCtrl.BlendTime
                    });
                else if (component is SpriteAnimatorController spriteCtrl)
                    data.SpriteAnimatorControllers.Add(new SpriteAnimatorControllerData
                    {
                        IdleClip = spriteCtrl.IdleClip, WalkClip = spriteCtrl.WalkClip,
                        JumpClip = spriteCtrl.JumpClip, AttackClip = spriteCtrl.AttackClip,
                        AttackAction = spriteCtrl.AttackAction, FlipByVelocity = spriteCtrl.FlipByVelocity,
                        ArtFacesRight = spriteCtrl.ArtFacesRight, BlendTime = spriteCtrl.BlendTime
                    });
                else if (component is NavChaser chaser)
                    data.NavChasers.Add(new NavChaserData
                    {
                        TargetTag = chaser.TargetTag, Speed = chaser.Speed,
                        RepathInterval = chaser.RepathInterval, ArriveRadius = chaser.ArriveRadius,
                        AllowDiagonal = chaser.AllowDiagonal
                    });
                else if (component is AudioListener)
                    data.AudioListeners.Add(new AudioListenerData());
                else if (component is Light2D light)
                    data.Lights.Add(new Light2DData
                    {
                        Radius = light.Radius, Intensity = light.Intensity,
                        R = light.Color.R, G = light.Color.G, B = light.Color.B
                    });
                else if (component is AmbientLight amb)
                    data.AmbientLights.Add(new AmbientLightData { R = amb.Color.R, G = amb.Color.G, B = amb.Color.B });
                else if (component is TopDownController td)
                    data.TopDowns.Add(new TopDownData
                    {
                        MoveSpeed = td.MoveSpeed, UseKeyboard = td.UseKeyboard,
                        HalfWidth = td.HalfWidth, HalfHeight = td.HalfHeight
                    });
                else if (component is Health hp)
                    data.Healths.Add(new HealthData
                    {
                        Max = hp.Max, InvulnTime = hp.InvulnTime, SendOnHit = hp.SendOnHit,
                        SendOnDeath = hp.SendOnDeath, DestroyOnDeath = hp.DestroyOnDeath
                    });
                else if (component is Hurtbox hb)
                    data.Hurtboxes.Add(new HurtboxData
                    {
                        Width = hb.Width, Height = hb.Height, OffsetX = hb.Offset.X, OffsetY = hb.Offset.Y, Team = hb.Team
                    });
                else if (component is Hitbox hx)
                    data.Hitboxes.Add(new HitboxData
                    {
                        Width = hx.Width, Height = hx.Height, OffsetX = hx.Offset.X, OffsetY = hx.Offset.Y,
                        Team = hx.Team, Damage = hx.Damage, ActiveTime = hx.ActiveTime, ActivateOn = hx.ActivateOn
                    });
                else if (component is SpriteFlash sf)
                    data.SpriteFlashes.Add(new SpriteFlashData
                    {
                        R = sf.FlashColor.R, G = sf.FlashColor.G, B = sf.FlashColor.B, Duration = sf.Duration
                    });
                else if (component is Joint2D joint)
                    data.Joints.Add(new JointData
                    {
                        Kind = (int)joint.Kind, ConnectedTag = joint.ConnectedTag,
                        AnchorX = joint.Anchor.X, AnchorY = joint.Anchor.Y,
                        CollideConnected = joint.CollideConnected, Frequency = joint.Frequency, DampingRatio = joint.DampingRatio
                    });
                else if (component is ShadowCaster shadow)
                    data.ShadowCasters.Add(new ShadowCasterData
                    {
                        Width = shadow.Width, Height = shadow.Height, OffsetX = shadow.Offset.X, OffsetY = shadow.Offset.Y
                    });
                else if (component is YSort ysort)
                    data.YSorts.Add(new YSortData { Offset = ysort.Offset });
                else if (component is PrefabInstance prefab)
                    data.PrefabInstances.Add(new PrefabInstanceData { PrefabPath = prefab.PrefabPath });
                else if (component is PostProcess post)
                    data.PostProcesses.Add(new PostProcessData { Saturation = post.Saturation, R = post.Tint.R, G = post.Tint.G, B = post.Tint.B });
                else if (component is LayeredMusic music)
                    data.LayeredMusics.Add(new LayeredMusicData
                    {
                        BaseTrackPath = music.BaseTrackPath,
                        BaseVolume = music.BaseVolume,
                        Bus = music.Bus,
                        Layers = music.Layers.Select(l => new MusicLayerData
                        {
                            TrackPath = l.TrackPath,
                            Tag = l.Tag,
                            MinCount = l.MinCount,
                            OnlyOnScreen = l.OnlyOnScreen,
                            RiseMessage = l.RiseMessage,
                            FallMessage = l.FallMessage,
                            FadeTime = l.FadeTime,
                            MaxVolume = l.MaxVolume
                        }).ToList()
                    });
                else if (component is ScreenFade fade)
                    data.ScreenFades.Add(new ScreenFadeData { R = fade.Color.R, G = fade.Color.G, B = fade.Color.B, Alpha = fade.Alpha, FlashOnMessage = fade.FlashOnMessage, FlashDuration = fade.FlashDuration });
                else if (component is AnimationStateMachine fsm)
                    data.StateMachines.Add(new AnimStateMachineData
                    {
                        DefaultState = fsm.DefaultState, BlendTime = fsm.BlendTime,
                        States = fsm.States.Select(x => new AnimStateData { Name = x.Name, Clip = x.Clip, X = x.X, Y = x.Y }).ToList(),
                        Transitions = fsm.Transitions.Select(x => new AnimTransData { From = x.From, To = x.To, Parameter = x.Parameter, Condition = (int)x.Condition }).ToList()
                    });
                else if (component is TweenComponent tween)
                    data.Tweens.Add(new TweenData
                    {
                        Channel = (int)tween.Channel,
                        From = tween.From, To = tween.To, Duration = tween.Duration,
                        Easing = (int)tween.Easing, Loop = (int)tween.Loop,
                        PlayOnStart = tween.PlayOnStart
                    });
                else if (component is TextRenderer text)
                    data.Texts.Add(new TextData
                    {
                        Text = text.Text,
                        LocKey = text.LocKey,
                        R = text.Color.R, G = text.Color.G, B = text.Color.B,
                        PixelSize = text.PixelSize,
                        ScreenSpace = text.ScreenSpace
                    });
                else if (component is CameraComponent cam)
                    data.Cameras.Add(new CameraData
                    {
                        TargetTag = cam.TargetTag,
                        DeadzoneWidth = cam.DeadzoneWidth,
                        DeadzoneHeight = cam.DeadzoneHeight,
                        SmoothTime = cam.SmoothTime,
                        Zoom = cam.Zoom,
                        UseBounds = cam.UseBounds,
                        BoundsMinX = cam.BoundsMin.X, BoundsMinY = cam.BoundsMin.Y,
                        BoundsMaxX = cam.BoundsMax.X, BoundsMaxY = cam.BoundsMax.Y,
                        Priority = cam.Priority, ShakeOnMessage = cam.ShakeOnMessage,
                        ShakeMessageDuration = cam.ShakeMessageDuration, ShakeMessageMagnitude = cam.ShakeMessageMagnitude
                    });
                else if (component is UiAnchor anchor)
                    data.Anchors.Add(new UiAnchorData
                    {
                        Anchor = (int)anchor.Anchor,
                        OffsetX = anchor.OffsetX,
                        OffsetY = anchor.OffsetY
                    });
                else if (component is UiButton button)
                    data.Buttons.Add(new UiButtonData
                    {
                        Width = button.Width,
                        Height = button.Height,
                        NR = button.Normal.R, NG = button.Normal.G, NB = button.Normal.B,
                        HR = button.Hover.R, HG = button.Hover.G, HB = button.Hover.B,
                        PR = button.Pressed.R, PG = button.Pressed.G, PB = button.Pressed.B,
                        SendOnClick = button.SendOnClick
                    });
                else if (component is ParallaxLayer parallax)
                    data.Parallaxes.Add(new ParallaxData { FactorX = parallax.FactorX, FactorY = parallax.FactorY });
                else if (component is TimerComponent timer)
                    data.Timers.Add(new TimerComponentData
                    {
                        Duration = timer.Duration,
                        Repeat = timer.Repeat,
                        AutoStart = timer.AutoStart,
                        SendOnElapsed = timer.SendOnElapsed,
                        StartOn = timer.StartOn
                    });
                else if (component is UiLayout layout)
                    data.Layouts.Add(new UiLayoutData { Direction = (int)layout.Direction, Spacing = layout.Spacing });
                else if (component is UiSlider slider)
                    data.Sliders.Add(new UiSliderData
                    {
                        Value = slider.Value, Width = slider.Width, Height = slider.Height,
                        TR = slider.Track.R, TG = slider.Track.G, TB = slider.Track.B,
                        FR = slider.Fill.R, FG = slider.Fill.G, FB = slider.Fill.B,
                        KR = slider.Knob.R, KG = slider.Knob.G, KB = slider.Knob.B,
                        BusTarget = slider.BusTarget, SendOnChange = slider.SendOnChange
                    });
                else if (component is UiToggle toggle)
                    data.Toggles.Add(new UiToggleData
                    {
                        IsOn = toggle.IsOn, Size = toggle.Size,
                        BR = toggle.Box.R, BG = toggle.Box.G, BB = toggle.Box.B,
                        CR = toggle.Check.R, CG = toggle.Check.G, CB = toggle.Check.B,
                        SendOnChange = toggle.SendOnChange
                    });
                else if (component is UiProgressBar bar)
                    data.ProgressBars.Add(new UiProgressBarData
                    {
                        Value = bar.Value, Width = bar.Width, Height = bar.Height,
                        TR = bar.Track.R, TG = bar.Track.G, TB = bar.Track.B,
                        FR = bar.Fill.R, FG = bar.Fill.G, FB = bar.Fill.B
                    });
                else if (component is UiTextField field)
                    data.TextFields.Add(new UiTextFieldData
                    {
                        Text = field.Text, Placeholder = field.Placeholder,
                        Width = field.Width, Height = field.Height,
                        PixelSize = field.PixelSize, MaxLength = field.MaxLength,
                        SendOnSubmit = field.SendOnSubmit
                    });
                else if (component is UiNavigator nav)
                    data.Navigators.Add(new UiNavigatorData { AutoFocusFirst = nav.AutoFocusFirst });
                else if (component is UiScrollView scroll)
                    data.ScrollViews.Add(new UiScrollViewData
                    {
                        Width = scroll.Width, Height = scroll.Height, Spacing = scroll.Spacing, ScrollSpeed = scroll.ScrollSpeed,
                        BR = scroll.Background.R, BG = scroll.Background.G, BB = scroll.Background.B
                    });
                else if (component is PropertyAnimator panim)
                    data.PropertyAnimators.Add(new PropertyAnimatorData
                    {
                        Duration = panim.Duration,
                        Loop = (int)panim.Loop,
                        PlayOnStart = panim.PlayOnStart,
                        Tracks = panim.Tracks.Select(tr => new PropertyTrackData
                        {
                            Channel = (int)tr.Channel,
                            Easing = (int)tr.Easing,
                            Keys = tr.Keys.SelectMany(k => new[] { k.Time, k.Value }).ToArray()
                        }).ToList()
                    });
                else if (component is Rigidbody2D rb)
                    data.Rigidbodies.Add(new RigidbodyData
                    {
                        Kind = (int)rb.Kind,
                        Shape = (int)rb.Shape,
                        Width = rb.Width, Height = rb.Height, Radius = rb.Radius,
                        Density = rb.Density, Friction = rb.Friction, Restitution = rb.Restitution,
                        FixedRotation = rb.FixedRotation,
                        CollisionCategory = rb.CollisionCategory, CollidesWith = rb.CollidesWith
                    });
            }

            foreach (var child in obj.Children)
                data.Children.Add(ToData(child));

            return data;
        }

        // ---- dados -> modelo ----

        private static Scene FromData(SceneData data)
        {
            var scene = new Scene { Name = data.Name };
            foreach (var objData in data.Objects)
                scene.Add(FromData(objData));
            foreach (var ledgeData in data.Ledges)
                scene.AddLedge(FromData(ledgeData));
            return scene;
        }

        private static Ledge FromData(LedgeData data)
        {
            var ledge = new Ledge { Name = data.Name, OneWay = data.OneWay };
            ledge.SetPoints(data.Points.Select(p => new Vector2(p.X, p.Y)));
            return ledge;
        }

        private static GameObject FromData(GameObjectData data)
        {
            var obj = new GameObject(data.Name) { Id = data.Id, Tag = data.Tag, SortOrder = data.SortOrder, RenderLayer = data.RenderLayer, ScreenSpace = data.ScreenSpace, IsVisible = data.IsVisible, Locked = data.Locked };
            obj.Transform.Position = new Vector2(data.PositionX, data.PositionY);
            obj.Transform.Rotation = data.Rotation;
            obj.Transform.Scale = new Vector2(data.ScaleX, data.ScaleY);

            foreach (var sprite in data.Sprites)
                obj.AddComponent(new SpriteRenderer
                {
                    Size = new Vector2(sprite.Width, sprite.Height),
                    Color = new Color(sprite.R, sprite.G, sprite.B, sprite.A),
                    TexturePath = sprite.TexturePath,
                    SourceRect = sprite.SrcW > 0 && sprite.SrcH > 0
                        ? new Rectangle(sprite.SrcX, sprite.SrcY, sprite.SrcW, sprite.SrcH)
                        : null,
                    ChromaKeyEnabled = sprite.ChromaKey,
                    ChromaAuto = sprite.ChromaAuto,
                    ChromaColor = new Color(sprite.ChromaR, sprite.ChromaG, sprite.ChromaB),
                    ChromaTolerance = sprite.ChromaTolerance
                });

            foreach (var rotator in data.Rotators)
                obj.AddComponent(new RotatorBehavior { Speed = rotator.Speed });

            foreach (var animator in data.Animators)
            {
                var anim = new SpriteAnimator
                {
                    TexturePath = animator.TexturePath,
                    FrameWidth = animator.FrameWidth,
                    FrameHeight = animator.FrameHeight,
                    FrameCount = animator.FrameCount,
                    Fps = animator.Fps,
                    Loop = animator.Loop,
                    Size = new Vector2(animator.Width, animator.Height),
                    ChromaKeyEnabled = animator.ChromaKey,
                    ChromaAuto = animator.ChromaAuto,
                    ChromaColor = new Color(animator.ChromaR, animator.ChromaG, animator.ChromaB),
                    ChromaTolerance = animator.ChromaTolerance
                };
                anim.SetEvents(animator.Events.Select(ev => new AnimationFrameEvent(ev.Frame, ev.Name)));
                anim.SetFrames(FramesFromFlat(animator.Frames));
                anim.SetClips(animator.Clips.Select(c => new SpriteClip(c.Name, c.Frames, c.Fps, c.Loop)));
                anim.FlipX = animator.FlipX;
                if (!string.IsNullOrEmpty(animator.DefaultClip))
                    anim.Play(animator.DefaultClip);
                obj.AddComponent(anim);
            }

            foreach (var tilemap in data.Tilemaps)
                obj.AddComponent(FromData(tilemap));

            foreach (var audio in data.Audios)
                obj.AddComponent(new AudioSource
                {
                    SoundPath = audio.SoundPath,
                    Volume = audio.Volume,
                    PlayOnStart = audio.PlayOnStart,
                    Loop = audio.Loop,
                    Bus = audio.Bus,
                    Spatial = audio.Spatial,
                    MaxDistance = audio.MaxDistance
                });

            foreach (var _ in data.AudioListeners)
                obj.AddComponent(new AudioListener());

            foreach (var light in data.Lights)
                obj.AddComponent(new Light2D
                {
                    Radius = light.Radius, Intensity = light.Intensity,
                    Color = new Color(light.R, light.G, light.B)
                });

            foreach (var amb in data.AmbientLights)
                obj.AddComponent(new AmbientLight { Color = new Color(amb.R, amb.G, amb.B) });

            foreach (var td in data.TopDowns)
                obj.AddComponent(new TopDownController
                {
                    MoveSpeed = td.MoveSpeed, UseKeyboard = td.UseKeyboard,
                    HalfWidth = td.HalfWidth, HalfHeight = td.HalfHeight
                });

            foreach (var hp in data.Healths)
                obj.AddComponent(new Health
                {
                    Max = hp.Max, InvulnTime = hp.InvulnTime, SendOnHit = hp.SendOnHit,
                    SendOnDeath = hp.SendOnDeath, DestroyOnDeath = hp.DestroyOnDeath
                });

            foreach (var hb in data.Hurtboxes)
                obj.AddComponent(new Hurtbox
                {
                    Width = hb.Width, Height = hb.Height, Offset = new Vector2(hb.OffsetX, hb.OffsetY), Team = hb.Team
                });

            foreach (var hx in data.Hitboxes)
                obj.AddComponent(new Hitbox
                {
                    Width = hx.Width, Height = hx.Height, Offset = new Vector2(hx.OffsetX, hx.OffsetY),
                    Team = hx.Team, Damage = hx.Damage, ActiveTime = hx.ActiveTime, ActivateOn = hx.ActivateOn
                });

            foreach (var sf in data.SpriteFlashes)
                obj.AddComponent(new SpriteFlash { FlashColor = new Color(sf.R, sf.G, sf.B), Duration = sf.Duration });

            foreach (var j in data.Joints)
                obj.AddComponent(new Joint2D
                {
                    Kind = (Joint2DKind)j.Kind, ConnectedTag = j.ConnectedTag,
                    Anchor = new Vector2(j.AnchorX, j.AnchorY),
                    CollideConnected = j.CollideConnected, Frequency = j.Frequency, DampingRatio = j.DampingRatio
                });

            foreach (var s in data.ShadowCasters)
                obj.AddComponent(new ShadowCaster
                {
                    Width = s.Width, Height = s.Height, Offset = new Vector2(s.OffsetX, s.OffsetY)
                });

            foreach (var ys in data.YSorts)
                obj.AddComponent(new YSort { Offset = ys.Offset });

            foreach (var pi in data.PrefabInstances)
                obj.AddComponent(new PrefabInstance { PrefabPath = pi.PrefabPath });

            foreach (var pp in data.PostProcesses)
                obj.AddComponent(new PostProcess { Saturation = pp.Saturation, Tint = new Color(pp.R, pp.G, pp.B) });

            foreach (var music in data.LayeredMusics)
            {
                var comp = new LayeredMusic
                {
                    BaseTrackPath = music.BaseTrackPath,
                    BaseVolume = music.BaseVolume,
                    Bus = music.Bus
                };
                foreach (var l in music.Layers)
                    comp.AddLayer(new MusicLayer
                    {
                        TrackPath = l.TrackPath,
                        Tag = l.Tag,
                        MinCount = l.MinCount,
                        OnlyOnScreen = l.OnlyOnScreen,
                        RiseMessage = l.RiseMessage,
                        FallMessage = l.FallMessage,
                        FadeTime = l.FadeTime,
                        MaxVolume = l.MaxVolume
                    });
                obj.AddComponent(comp);
            }

            foreach (var f in data.ScreenFades)
                obj.AddComponent(new ScreenFade { Color = new Color(f.R, f.G, f.B), Alpha = f.Alpha, FlashOnMessage = f.FlashOnMessage, FlashDuration = f.FlashDuration });

            foreach (var sm in data.StateMachines)
            {
                var fsm = new AnimationStateMachine { DefaultState = sm.DefaultState, BlendTime = sm.BlendTime };
                fsm.SetStates(sm.States.Select(x => new AnimStateDef { Name = x.Name, Clip = x.Clip, X = x.X, Y = x.Y }));
                fsm.SetTransitions(sm.Transitions.Select(x => new AnimTransitionDef { From = x.From, To = x.To, Parameter = x.Parameter, Condition = (AnimCondition)x.Condition }));
                obj.AddComponent(fsm);
            }

            foreach (var chaser in data.NavChasers)
                obj.AddComponent(new NavChaser
                {
                    TargetTag = chaser.TargetTag, Speed = chaser.Speed,
                    RepathInterval = chaser.RepathInterval, ArriveRadius = chaser.ArriveRadius,
                    AllowDiagonal = chaser.AllowDiagonal
                });

            foreach (var particles in data.Particles)
                obj.AddComponent(new ParticleEmitter
                {
                    EmitRate = particles.EmitRate,
                    Lifetime = particles.Lifetime,
                    Speed = particles.Speed,
                    Spread = particles.Spread,
                    Size = particles.Size,
                    GravityY = particles.GravityY,
                    Color = new Color(particles.R, particles.G, particles.B),
                    EndColor = new Color(particles.EndR, particles.EndG, particles.EndB),
                    EndSize = particles.EndSize, BurstCount = particles.BurstCount, EmitOnStart = particles.EmitOnStart
                });

            foreach (var follow in data.Follows)
                obj.AddComponent(new FollowTarget { TargetId = follow.TargetId, Speed = follow.Speed });

            foreach (var trigger in data.Triggers)
                obj.AddComponent(new TriggerZone
                {
                    Size = new Vector2(trigger.Width, trigger.Height),
                    Color = new Color(trigger.R, trigger.G, trigger.B),
                    TargetTag = trigger.TargetTag,
                    DestroyOnEnter = trigger.DestroyOnEnter,
                    SendOnEnter = trigger.SendOnEnter
                });

            foreach (var script in data.Scripts)
                obj.AddComponent(new ScriptComponent { Source = script.Source, SourcePath = script.SourcePath });

            foreach (var bone in data.Bones)
                obj.AddComponent(new Bone
                {
                    Length = bone.Length,
                    RestPosition = new Vector2(bone.RestX, bone.RestY),
                    RestRotation = bone.RestRotation,
                    RestScale = new Vector2(bone.RestScaleX, bone.RestScaleY),
                    HasRestPose = bone.HasRestPose
                });

            foreach (var listener in data.MessageListeners)
                obj.AddComponent(new MessageListener
                {
                    Message = listener.Message,
                    Reaction = (MessageReaction)listener.Reaction
                });

            foreach (var skel in data.Skeletons)
            {
                var animator = new SkeletonAnimator();

                // Formato novo (clipes) ou legado (single-clip) via um clipe "default".
                var clipDatas = skel.Clips.Count > 0
                    ? skel.Clips
                    : new System.Collections.Generic.List<PoseClipData>
                      {
                          new() { Name = "default", Duration = skel.Duration, Loop = skel.Loop,
                                  Easing = skel.Easing, Keyframes = skel.Keyframes, Events = skel.Events }
                      };

                bool first = true;
                foreach (var clipData in clipDatas)
                {
                    // O animador já nasce com "default"; renomeia-o no primeiro clipe.
                    if (first) { animator.CurrentClip.Name = clipData.Name; first = false; }
                    else animator.AddClip(clipData.Name);

                    animator.Duration = clipData.Duration;
                    animator.Loop = clipData.Loop;
                    animator.Easing = (Scrawlbit.EasingMode)clipData.Easing;
                    animator.SetEvents(clipData.Events.Select(ev => (ev.Time, ev.Name)));
                    foreach (var kf in clipData.Keyframes)
                    {
                        var bones = new System.Collections.Generic.Dictionary<string, DreamBit.Engine.Animation.BonePose>();
                        foreach (var b in kf.Bones)
                            bones[b.Bone] = new DreamBit.Engine.Animation.BonePose(
                                new Vector2(b.Px, b.Py), b.Rot, new Vector2(b.Sx, b.Sy));
                        animator.AddKeyframe(new DreamBit.Engine.Animation.PoseKeyframe(kf.Time, bones));
                    }
                }

                animator.CurrentClipName = skel.CurrentClip;
                obj.AddComponent(animator);
            }

            foreach (var platformer in data.Platformers)
                obj.AddComponent(new PlatformerController
                {
                    Gravity = platformer.Gravity,
                    HalfHeight = platformer.HalfHeight,
                    HalfWidth = platformer.HalfWidth,
                    HorizontalSpeed = platformer.HorizontalSpeed,
                    UseKeyboard = platformer.UseKeyboard,
                    MoveSpeed = platformer.MoveSpeed,
                    JumpSpeed = platformer.JumpSpeed
                });

            foreach (var box in data.BoxColliders)
                obj.AddComponent(new BoxCollider
                {
                    Size = new Vector2(box.Width, box.Height),
                    Offset = new Vector2(box.OffsetX, box.OffsetY)
                });

            foreach (var exit in data.SceneExits)
                obj.AddComponent(new SceneExit
                {
                    Size = new Vector2(exit.Width, exit.Height),
                    TargetTag = exit.TargetTag,
                    TargetScene = exit.TargetScene
                });

            foreach (var ac in data.AnimatorControllers)
                obj.AddComponent(new AnimatorController
                {
                    IdleClip = ac.IdleClip, WalkClip = ac.WalkClip, JumpClip = ac.JumpClip, BlendTime = ac.BlendTime
                });

            foreach (var sc in data.SpriteAnimatorControllers)
                obj.AddComponent(new SpriteAnimatorController
                {
                    IdleClip = sc.IdleClip, WalkClip = sc.WalkClip, JumpClip = sc.JumpClip,
                    AttackClip = sc.AttackClip, AttackAction = sc.AttackAction,
                    FlipByVelocity = sc.FlipByVelocity, ArtFacesRight = sc.ArtFacesRight, BlendTime = sc.BlendTime
                });

            foreach (var tween in data.Tweens)
                obj.AddComponent(new TweenComponent
                {
                    Channel = (TweenChannel)tween.Channel,
                    From = tween.From, To = tween.To, Duration = tween.Duration,
                    Easing = (Scrawlbit.EasingMode)tween.Easing, Loop = (TweenLoop)tween.Loop,
                    PlayOnStart = tween.PlayOnStart
                });

            foreach (var text in data.Texts)
                obj.AddComponent(new TextRenderer
                {
                    Text = text.Text,
                    LocKey = text.LocKey,
                    Color = new Color(text.R, text.G, text.B),
                    PixelSize = text.PixelSize,
                    ScreenSpace = text.ScreenSpace
                });

            foreach (var cam in data.Cameras)
                obj.AddComponent(new CameraComponent
                {
                    TargetTag = cam.TargetTag,
                    DeadzoneWidth = cam.DeadzoneWidth,
                    DeadzoneHeight = cam.DeadzoneHeight,
                    SmoothTime = cam.SmoothTime,
                    Zoom = cam.Zoom,
                    UseBounds = cam.UseBounds,
                    BoundsMin = new Vector2(cam.BoundsMinX, cam.BoundsMinY),
                    BoundsMax = new Vector2(cam.BoundsMaxX, cam.BoundsMaxY),
                    Priority = cam.Priority, ShakeOnMessage = cam.ShakeOnMessage,
                    ShakeMessageDuration = cam.ShakeMessageDuration, ShakeMessageMagnitude = cam.ShakeMessageMagnitude
                });

            foreach (var anchor in data.Anchors)
                obj.AddComponent(new UiAnchor
                {
                    Anchor = (AnchorPoint)anchor.Anchor,
                    OffsetX = anchor.OffsetX,
                    OffsetY = anchor.OffsetY
                });

            foreach (var button in data.Buttons)
                obj.AddComponent(new UiButton
                {
                    Width = button.Width,
                    Height = button.Height,
                    Normal = new Color(button.NR, button.NG, button.NB),
                    Hover = new Color(button.HR, button.HG, button.HB),
                    Pressed = new Color(button.PR, button.PG, button.PB),
                    SendOnClick = button.SendOnClick
                });

            foreach (var parallax in data.Parallaxes)
                obj.AddComponent(new ParallaxLayer { FactorX = parallax.FactorX, FactorY = parallax.FactorY });

            foreach (var timer in data.Timers)
                obj.AddComponent(new TimerComponent
                {
                    Duration = timer.Duration,
                    Repeat = timer.Repeat,
                    AutoStart = timer.AutoStart,
                    SendOnElapsed = timer.SendOnElapsed,
                    StartOn = timer.StartOn
                });

            foreach (var layout in data.Layouts)
                obj.AddComponent(new UiLayout { Direction = (LayoutDirection)layout.Direction, Spacing = layout.Spacing });

            foreach (var s in data.Sliders)
                obj.AddComponent(new UiSlider
                {
                    Value = s.Value, Width = s.Width, Height = s.Height,
                    Track = new Color(s.TR, s.TG, s.TB), Fill = new Color(s.FR, s.FG, s.FB), Knob = new Color(s.KR, s.KG, s.KB),
                    BusTarget = s.BusTarget, SendOnChange = s.SendOnChange
                });

            foreach (var t in data.Toggles)
                obj.AddComponent(new UiToggle
                {
                    IsOn = t.IsOn, Size = t.Size,
                    Box = new Color(t.BR, t.BG, t.BB), Check = new Color(t.CR, t.CG, t.CB),
                    SendOnChange = t.SendOnChange
                });

            foreach (var b in data.ProgressBars)
                obj.AddComponent(new UiProgressBar
                {
                    Value = b.Value, Width = b.Width, Height = b.Height,
                    Track = new Color(b.TR, b.TG, b.TB), Fill = new Color(b.FR, b.FG, b.FB)
                });

            foreach (var f in data.TextFields)
                obj.AddComponent(new UiTextField
                {
                    Text = f.Text, Placeholder = f.Placeholder,
                    Width = f.Width, Height = f.Height,
                    PixelSize = f.PixelSize, MaxLength = f.MaxLength,
                    SendOnSubmit = f.SendOnSubmit
                });

            foreach (var n in data.Navigators)
                obj.AddComponent(new UiNavigator { AutoFocusFirst = n.AutoFocusFirst });

            foreach (var sc in data.ScrollViews)
                obj.AddComponent(new UiScrollView
                {
                    Width = sc.Width, Height = sc.Height, Spacing = sc.Spacing, ScrollSpeed = sc.ScrollSpeed,
                    Background = new Color(sc.BR, sc.BG, sc.BB)
                });

            foreach (var pa in data.PropertyAnimators)
            {
                var animator = new PropertyAnimator { Duration = pa.Duration, Loop = (TweenLoop)pa.Loop, PlayOnStart = pa.PlayOnStart };
                foreach (var td in pa.Tracks)
                {
                    var track = new PropertyTrack { Channel = (AnimChannel)td.Channel, Easing = (Scrawlbit.EasingMode)td.Easing };
                    for (int i = 0; i + 1 < td.Keys.Length; i += 2)
                        track.Keys.Add(new AnimKey(td.Keys[i], td.Keys[i + 1]));
                    animator.Tracks.Add(track);
                }
                obj.AddComponent(animator);
            }

            foreach (var rb in data.Rigidbodies)
                obj.AddComponent(new Rigidbody2D
                {
                    Kind = (RigidbodyKind)rb.Kind,
                    Shape = (ColliderShape)rb.Shape,
                    Width = rb.Width, Height = rb.Height, Radius = rb.Radius,
                    Density = rb.Density, Friction = rb.Friction, Restitution = rb.Restitution,
                    FixedRotation = rb.FixedRotation,
                    CollisionCategory = rb.CollisionCategory, CollidesWith = rb.CollidesWith
                });

            foreach (var childData in data.Children)
                obj.AddChild(FromData(childData));

            return obj;
        }
    }
}
