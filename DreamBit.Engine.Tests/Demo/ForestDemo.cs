using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using DreamBit.Engine.Audio;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests.Demo
{
    /// <summary>
    /// Monta a fase de demonstração "Floresta" usando os assets reais (Dust: An Elysian Tail):
    /// o herói animado por clipes (andar/pular/bater) recortado da sprite sheet e o cenário
    /// composto do atlas de floresta. Exercita, numa cena só, praticamente toda a engine:
    /// tilemap sólido, física de plataforma, câmera, HUD/UI, parallax, pathfinding, física
    /// rígida, timeline de propriedades, triggers/mensagens e áudio espacial.
    /// </summary>
    public static class ForestDemo
    {
        public const string AssetsDir = @"C:/Projetos/dreambit/DemoAssets";
        private const string Hero = "DemoAssets/hero.png";
        private const string Atlas = "DemoAssets/forest-atlas.png";
        private const string GroundTile = "DemoAssets/ground.png";
        private const int Tile = 96;

        // Faixas de frames da sheet do herói -> clipes (índices globais na lista detectada).
        private static readonly (string Name, int Start, int Count, float Fps, bool Loop)[] Clips =
        {
            ("idle",   65, 13, 8f,  true),
            ("walk",    0, 13, 14f, true),
            ("jump",   39, 13, 12f, false),
            ("attack",130, 13, 16f, false),
            ("death", 144, 12, 12f, false),
        };

        public sealed record HeroFrames(int[] Sheet, List<int[]> Rects);

        /// <summary>Lê os retângulos de frame detectados (DemoAssets/hero-frames.json).</summary>
        public static List<Rectangle> LoadHeroFrames(string? dir = null)
        {
            string path = Path.Combine(dir ?? AssetsDir, "hero-frames.json");
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            var frames = new List<Rectangle>();
            foreach (var f in doc.RootElement.GetProperty("frames").EnumerateArray())
                frames.Add(new Rectangle(
                    f.GetProperty("x").GetInt32(), f.GetProperty("y").GetInt32(),
                    f.GetProperty("w").GetInt32(), f.GetProperty("h").GetInt32()));
            return frames;
        }

        public static Scene Build(string? assetsDir = null)
        {
            var scene = new Scene();

            BuildParallaxBackground(scene);
            BuildGround(scene);
            BuildDecorations(scene);
            var hero = BuildHero(scene, assetsDir);
            BuildCamera(scene);
            BuildEnemy(scene);
            BuildPickup(scene);
            BuildPhysicsCrates(scene);
            BuildGoal(scene);
            BuildLights(scene);
            BuildHud(scene);

            return scene;
        }

        private static GameObject Sprite(Scene scene, string name, Vector2 pos, Vector2 size,
            Rectangle src, int layer, string texture = Atlas)
        {
            var o = new GameObject(name) { RenderLayer = layer };
            o.Transform.Position = pos;
            o.AddComponent(new SpriteRenderer { TexturePath = texture, Size = size, SourceRect = src, Color = Color.White });
            scene.Add(o);
            return o;
        }

        private static void BuildParallaxBackground(Scene scene)
        {
            // Backdrop distante + colina, com parallax (andam menos que a câmera).
            var back = Sprite(scene, "Backdrop", new Vector2(1000, 260), new Vector2(1600, 1050),
                new Rectangle(0, 2765, 1006, 659), layer: -20);
            back.AddComponent(new ParallaxLayer { FactorX = 0.25f, FactorY = 0.25f });

            var hill = Sprite(scene, "Colina", new Vector2(1200, 520), new Vector2(1550, 494),
                new Rectangle(1056, 2265, 1555, 494), layer: -15);
            hill.AddComponent(new ParallaxLayer { FactorX = 0.5f, FactorY = 0.5f });
        }

        private static void BuildGround(Scene scene)
        {
            var map = new Tilemap.Tilemap { TileWidth = Tile, TileHeight = Tile };
            map.Tilesets.Add(new Tilemap.Tileset
            {
                FirstGid = 1, Columns = 1, TileCount = 1,
                TileWidth = Tile, TileHeight = Tile,
                ImageSource = GroundTile, ResolvedImagePath = GroundTile
            });
            var layer = map.PaintLayer();
            layer.Name = "Chao";
            for (int x = 0; x <= 26; x++)      // piso
                layer.SetTile(x, 6, 1);
            for (int x = 0; x <= 26; x++)      // subsolo (colisão dupla)
                layer.SetTile(x, 7, 1);
            for (int y = 4; y <= 5; y++)       // pilar (obstáculo de pathfinding)
                layer.SetTile(13, y, 1);

            var o = new GameObject("Terreno") { RenderLayer = -5 };
            o.AddComponent(new TilemapRenderer { Map = map, Solid = true, Edited = true });
            scene.Add(o);
        }

        private static void BuildDecorations(Scene scene)
        {
            Sprite(scene, "ArvoreGrande", new Vector2(650, 250), new Vector2(705, 488),
                new Rectangle(2142, 359, 1410, 975), layer: -8);
            Sprite(scene, "ArvoreFrondosa", new Vector2(1500, 300), new Vector2(453, 471),
                new Rectangle(5, 1457, 756, 785), layer: -6);
            Sprite(scene, "Moita", new Vector2(400, 470), new Vector2(360, 196),
                new Rectangle(4, 2265, 903, 490), layer: 2);
            Sprite(scene, "Rochas", new Vector2(1850, 470), new Vector2(342, 317),
                new Rectangle(2098, 1352, 854, 793), layer: 1);
        }

        private static GameObject BuildHero(Scene scene, string? assetsDir)
        {
            var hero = new GameObject("Heroi") { Tag = "player", RenderLayer = 0 };
            hero.Transform.Position = new Vector2(300, 300);

            var anim = new SpriteAnimator
            {
                TexturePath = Hero,
                Size = new Vector2(200, 200),
                Fps = 12f,
            };
            anim.SetFrames(LoadHeroFrames(assetsDir));
            foreach (var (name, start, count, fps, loop) in Clips)
                anim.AddClip(SpriteClip.Range(name, start, count, fps, loop));
            anim.Play("idle");
            // Evento no frame do golpe (dentro do clipe attack) que ativa a hitbox.
            anim.SetEvents(new[] { new AnimationFrameEvent(135, "hit") });
            hero.AddComponent(anim);

            // Hitbox de ataque (time 0), ativada pelo evento "hit" do clipe de ataque.
            hero.AddComponent(new Hitbox { Team = 0, Damage = 20f, Width = 150f, Height = 170f, Offset = new Vector2(80, 0), ActivateOn = "hit" });

            hero.AddComponent(new SpriteAnimatorController
            {
                IdleClip = "idle", WalkClip = "walk", JumpClip = "jump",
                AttackClip = "attack", AttackAction = "Action", ArtFacesRight = true
            });

            hero.AddComponent(new PlatformerController
            {
                UseKeyboard = true, MoveSpeed = 240f, JumpSpeed = 720f, Gravity = 1800f,
                HalfWidth = 40f, HalfHeight = 95f
            });

            hero.AddComponent(new AudioListener());
            hero.AddComponent(new Light2D { Radius = 340f, Color = new Color(255, 236, 200), Intensity = 1.15f });
            scene.Add(hero);
            return hero;
        }

        private static void BuildCamera(Scene scene)
        {
            var cam = new GameObject("Camera");
            cam.Transform.Position = new Vector2(300, 300);
            cam.AddComponent(new CameraComponent
            {
                TargetTag = "player", DeadzoneWidth = 120f, DeadzoneHeight = 90f,
                SmoothTime = 0.15f, Zoom = 1f
            });
            scene.Add(cam);
        }

        private static void BuildEnemy(Scene scene)
        {
            var enemy = new GameObject("Inimigo") { Tag = "enemy", RenderLayer = 0 };
            enemy.Transform.Position = new Vector2(1700, 150);
            enemy.AddComponent(new SpriteRenderer
            {
                TexturePath = Atlas, Size = new Vector2(120, 120),
                SourceRect = new Rectangle(3575, 348, 236, 239), Color = new Color(180, 220, 255)
            });
            enemy.AddComponent(new NavChaser { TargetTag = "player", Speed = 150f, RepathInterval = 0.4f });
            // Combate: vida + área que recebe golpes (time 1) + piscar ao levar dano.
            enemy.AddComponent(new Health { Max = 60f, InvulnTime = 0.25f, DestroyOnDeath = true, SendOnDeath = "enemy_down" });
            enemy.AddComponent(new Hurtbox { Team = 1, Width = 120f, Height = 120f });
            enemy.AddComponent(new SpriteFlash { FlashColor = new Color(255, 90, 90), Duration = 0.12f });
            scene.Add(enemy);
        }

        private static void BuildPickup(Scene scene)
        {
            var pickup = new GameObject("Cristal") { RenderLayer = 1 };
            pickup.Transform.Position = new Vector2(1050, 420);
            pickup.AddComponent(new SpriteRenderer
            {
                TexturePath = Atlas, Size = new Vector2(70, 70),
                SourceRect = new Rectangle(3575, 348, 236, 239), Color = new Color(120, 255, 200)
            });
            // Timeline: flutua (sobe/desce) e pulsa o alpha.
            var anim = new PropertyAnimator { Duration = 1.6f, Loop = TweenLoop.PingPong, PlayOnStart = true };
            var bob = new PropertyTrack { Channel = AnimChannel.PositionY };
            bob.Keys.Add(new AnimKey(0f, 420f));
            bob.Keys.Add(new AnimKey(1.6f, 380f));
            var alpha = new PropertyTrack { Channel = AnimChannel.SpriteAlpha };
            alpha.Keys.Add(new AnimKey(0f, 0.6f));
            alpha.Keys.Add(new AnimKey(1.6f, 1f));
            anim.Tracks.Add(bob);
            anim.Tracks.Add(alpha);
            pickup.AddComponent(anim);
            // Som ambiente espacial (some ao se afastar do herói).
            pickup.AddComponent(new AudioSource { Spatial = true, MaxDistance = 500f, Loop = true, Bus = AudioMixer.Sfx });
            pickup.AddComponent(new Light2D { Radius = 240f, Color = new Color(120, 255, 220), Intensity = 1.3f });
            scene.Add(pickup);
        }

        private static void BuildLights(Scene scene)
        {
            // Ambiente de fim de tarde (a cena fica em penumbra; as luzes abrem clareiras).
            var amb = new GameObject("Ambiente");
            amb.AddComponent(new AmbientLight { Color = new Color(82, 88, 116) });
            scene.Add(amb);

            // Luz quente na tenda (meta).
            var tentLight = new GameObject("LuzTenda");
            tentLight.Transform.Position = new Vector2(2300, 430);
            tentLight.AddComponent(new Light2D { Radius = 300f, Color = new Color(255, 210, 150), Intensity = 1.1f });
            scene.Add(tentLight);

            // Oclusores: o tronco da árvore e as rochas projetam sombra das luzes.
            var trunk = new GameObject("SombraArvore");
            trunk.Transform.Position = new Vector2(650, 430);
            trunk.AddComponent(new ShadowCaster { Width = 90f, Height = 180f });
            scene.Add(trunk);

            var rockShadow = new GameObject("SombraRocha");
            rockShadow.Transform.Position = new Vector2(1850, 470);
            rockShadow.AddComponent(new ShadowCaster { Width = 260f, Height = 180f });
            scene.Add(rockShadow);
        }

        private static void BuildPhysicsCrates(Scene scene)
        {
            // Chão estático da física (para as caixas empilharem) + caixas dinâmicas.
            var floor = new GameObject("FisicaChao");
            floor.Transform.Position = new Vector2(750, 545);
            floor.AddComponent(new Rigidbody2D { Kind = RigidbodyKind.Static, Shape = ColliderShape.Box, Width = 400, Height = 40 });
            scene.Add(floor);

            for (int i = 0; i < 3; i++)
            {
                var crate = new GameObject("Caixa" + i) { RenderLayer = 1 };
                crate.Transform.Position = new Vector2(750, 300 - i * 70);
                crate.AddComponent(new SpriteRenderer { Size = new Vector2(60, 60), Color = new Color(150, 110, 70) });
                crate.AddComponent(new Rigidbody2D
                {
                    Kind = RigidbodyKind.Dynamic, Shape = ColliderShape.Box,
                    Width = 60, Height = 60, Density = 1f, Friction = 0.6f, Restitution = 0.05f
                });
                scene.Add(crate);
            }
        }

        private static void BuildGoal(Scene scene)
        {
            var tent = Sprite(scene, "Tenda", new Vector2(2300, 470), new Vector2(340, 198),
                new Rectangle(3515, 2432, 450, 262), layer: 1);

            var trigger = new GameObject("MetaTenda");
            trigger.Transform.Position = new Vector2(2300, 470);
            trigger.AddComponent(new TriggerZone
            {
                Size = new Vector2(200, 300), TargetTag = "player", SendOnEnter = "reach_tent"
            });
            scene.Add(trigger);
        }

        private static void BuildHud(Scene scene)
        {
            var hud = new GameObject("HUD") { ScreenSpace = true };
            scene.Add(hud);

            var barObj = new GameObject("Vida") { ScreenSpace = true };
            barObj.AddComponent(new UiAnchor { Anchor = AnchorPoint.TopLeft, OffsetX = 24, OffsetY = 24 });
            barObj.AddComponent(new UiProgressBar { Value = 0.8f, Width = 220, Height = 22, Fill = new Color(210, 70, 70) });
            hud.AddChild(barObj);

            var titleObj = new GameObject("Titulo") { ScreenSpace = true };
            titleObj.AddComponent(new UiAnchor { Anchor = AnchorPoint.TopLeft, OffsetX = 24, OffsetY = 58 });
            titleObj.AddComponent(new TextRenderer { LocKey = "hud.title", Text = "FLORESTA", PixelSize = 3, ScreenSpace = true, Color = Color.White });
            hud.AddChild(titleObj);

            var volObj = new GameObject("Volume") { ScreenSpace = true };
            volObj.AddComponent(new UiAnchor { Anchor = AnchorPoint.BottomLeft, OffsetX = 24, OffsetY = -40 });
            volObj.AddComponent(new UiSlider { Value = 0.7f, Width = 200, Height = 20, BusTarget = AudioMixer.Music });
            hud.AddChild(volObj);
        }
    }
}
