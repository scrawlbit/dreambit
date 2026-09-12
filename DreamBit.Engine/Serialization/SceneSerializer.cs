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

        private static TilemapData ToData(TilemapRenderer tilemap)
        {
            var data = new TilemapData { TmxPath = tilemap.TmxPath, Edited = tilemap.Edited };
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
                    data.Layers.Add(new TileLayerData { Name = layer.Name, Tiles = flat.ToArray() });
                }
            }

            return data;
        }

        private static TilemapRenderer FromData(TilemapData data)
        {
            var tilemap = new TilemapRenderer { TmxPath = data.TmxPath, Edited = data.Edited };

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
                    var layer = new TileLayer { Name = ld.Name };
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
                        TexturePath = sprite.TexturePath
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
                        Height = animator.Size.Y
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
                        DestroyOnEnter = trigger.DestroyOnEnter
                    });
                else if (component is ScriptComponent script)
                    data.Scripts.Add(new ScriptData { Source = script.Source });
                else if (component is PlatformerController platformer)
                    data.Platformers.Add(new PlatformerData
                    {
                        Gravity = platformer.Gravity,
                        HalfHeight = platformer.HalfHeight,
                        HorizontalSpeed = platformer.HorizontalSpeed,
                        UseKeyboard = platformer.UseKeyboard,
                        MoveSpeed = platformer.MoveSpeed,
                        JumpSpeed = platformer.JumpSpeed
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
            var obj = new GameObject(data.Name) { Id = data.Id, Tag = data.Tag, IsVisible = data.IsVisible };
            obj.Transform.Position = new Vector2(data.PositionX, data.PositionY);
            obj.Transform.Rotation = data.Rotation;
            obj.Transform.Scale = new Vector2(data.ScaleX, data.ScaleY);

            foreach (var sprite in data.Sprites)
                obj.AddComponent(new SpriteRenderer
                {
                    Size = new Vector2(sprite.Width, sprite.Height),
                    Color = new Color(sprite.R, sprite.G, sprite.B, sprite.A),
                    TexturePath = sprite.TexturePath
                });

            foreach (var rotator in data.Rotators)
                obj.AddComponent(new RotatorBehavior { Speed = rotator.Speed });

            foreach (var animator in data.Animators)
                obj.AddComponent(new SpriteAnimator
                {
                    TexturePath = animator.TexturePath,
                    FrameWidth = animator.FrameWidth,
                    FrameHeight = animator.FrameHeight,
                    FrameCount = animator.FrameCount,
                    Fps = animator.Fps,
                    Loop = animator.Loop,
                    Size = new Vector2(animator.Width, animator.Height)
                });

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
                    DestroyOnEnter = trigger.DestroyOnEnter
                });

            foreach (var script in data.Scripts)
                obj.AddComponent(new ScriptComponent { Source = script.Source });

            foreach (var platformer in data.Platformers)
                obj.AddComponent(new PlatformerController
                {
                    Gravity = platformer.Gravity,
                    HalfHeight = platformer.HalfHeight,
                    HorizontalSpeed = platformer.HorizontalSpeed,
                    UseKeyboard = platformer.UseKeyboard,
                    MoveSpeed = platformer.MoveSpeed,
                    JumpSpeed = platformer.JumpSpeed
                });

            foreach (var childData in data.Children)
                obj.AddChild(FromData(childData));

            return obj;
        }
    }
}
