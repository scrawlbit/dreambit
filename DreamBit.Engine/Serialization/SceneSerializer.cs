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
    /// limpo, com a serialização desacoplada — como faz o DreamBit.Game atual em
    /// Serialization/Converters, aqui reescrito para net8 sem dependência de Newtonsoft.
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

        public static Scene LoadFromString(string json)
            => FromData(JsonSerializer.Deserialize<SceneData>(json, Options) ?? new SceneData());

        // ---- Prefabs (um objeto isolado) ----

        public static void SaveObject(GameObject obj, string path)
            => File.WriteAllText(path, JsonSerializer.Serialize(ToData(obj), Options));

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
            var data = new TilemapData { TmxPath = tilemap.TmxPath, Edited = tilemap.Edited, Solid = tilemap.Solid, SolidLayer = tilemap.SolidLayer };
            var map = tilemap.Map;

            // Serializa o mapa inline quando pintado (ou quando não veio de .tmx).
            bool inline = map != null && (tilemap.Edited || string.IsNullOrEmpty(tilemap.TmxPath));
            if (inline && map != null)
            {
                data.TileWidth = map.TileWidth;
                data.TileHeight = map.TileHeight;

                foreach (var ts in map.Tilesets)
                    data.Tilesets.Add(new TilesetData
                    {
                        FirstGid = ts.FirstGid,
                        ImagePath = ts.ResolvedImagePath,
                        Columns = ts.Columns,
                        TileWidth = ts.TileWidth,
                        TileHeight = ts.TileHeight
                    });

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
            var tilemap = new TilemapRenderer { TmxPath = data.TmxPath, Edited = data.Edited, Solid = data.Solid, SolidLayer = data.SolidLayer };

            if (data.Tilesets.Count > 0 || data.Layers.Count > 0)
            {
                var map = new Tilemap.Tilemap { TileWidth = data.TileWidth, TileHeight = data.TileHeight };

                foreach (var ts in data.Tilesets)
                    map.Tilesets.Add(new Tileset
                    {
                        FirstGid = ts.FirstGid,
                        ResolvedImagePath = ts.ImagePath,
                        Columns = ts.Columns,
                        TileWidth = ts.TileWidth,
                        TileHeight = ts.TileHeight
                    });

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
                        SrcH = sprite.SourceRect?.Height ?? 0
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
                        Loop = audio.Loop
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
                        B = particles.Color.B
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
                        IdleClip = animCtrl.IdleClip, WalkClip = animCtrl.WalkClip, JumpClip = animCtrl.JumpClip
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
                        BoundsMaxX = cam.BoundsMax.X, BoundsMaxY = cam.BoundsMax.Y
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
                else if (component is Rigidbody2D rb)
                    data.Rigidbodies.Add(new RigidbodyData
                    {
                        Kind = (int)rb.Kind,
                        Shape = (int)rb.Shape,
                        Width = rb.Width, Height = rb.Height, Radius = rb.Radius,
                        Density = rb.Density, Friction = rb.Friction, Restitution = rb.Restitution,
                        FixedRotation = rb.FixedRotation
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
            var obj = new GameObject(data.Name) { Id = data.Id, Tag = data.Tag, SortOrder = data.SortOrder, RenderLayer = data.RenderLayer, ScreenSpace = data.ScreenSpace, IsVisible = data.IsVisible };
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
                        : null
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
                    Size = new Vector2(animator.Width, animator.Height)
                };
                anim.SetEvents(animator.Events.Select(ev => new AnimationFrameEvent(ev.Frame, ev.Name)));
                anim.SetFrames(FramesFromFlat(animator.Frames));
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
                    Loop = audio.Loop
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
                    Color = new Color(particles.R, particles.G, particles.B)
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
                    IdleClip = ac.IdleClip, WalkClip = ac.WalkClip, JumpClip = ac.JumpClip
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
                    BoundsMax = new Vector2(cam.BoundsMaxX, cam.BoundsMaxY)
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

            foreach (var rb in data.Rigidbodies)
                obj.AddComponent(new Rigidbody2D
                {
                    Kind = (RigidbodyKind)rb.Kind,
                    Shape = (ColliderShape)rb.Shape,
                    Width = rb.Width, Height = rb.Height, Radius = rb.Radius,
                    Density = rb.Density, Friction = rb.Friction, Restitution = rb.Restitution,
                    FixedRotation = rb.FixedRotation
                });

            foreach (var childData in data.Children)
                obj.AddChild(FromData(childData));

            return obj;
        }
    }
}
