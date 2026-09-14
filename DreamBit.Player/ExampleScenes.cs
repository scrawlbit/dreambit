using System;
using System.Collections.Generic;
using System.IO;
using DreamBit.Engine.Audio;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;

namespace DreamBit.Player
{
    /// <summary>
    /// Galeria de exemplos: uma cena curta por funcionalidade da engine, montada por código.
    /// Cada uma pode ser salva como .dbscene (abrir no editor) e rodada no Player para ver
    /// funcionando. A câmera fica na origem (view ~1280x720), então tudo é montado em torno de (0,0).
    /// </summary>
    public static class ExampleScenes
    {
        public static readonly (string Name, string Title, Func<Scene> Build)[] All =
        {
            ("01-sprites",       "Sprites e Z-Order",        Sprites),
            ("02-tween",         "Timeline / Tween",         Tween),
            ("03-sprite-anim",   "Animação de Sprite",       SpriteAnim),
            ("04-tilemap",       "Tilemap",                  Tilemap),
            ("05-lights",        "Luzes 2D + Sombras",       Lights),
            ("06-physics",       "Física + Joints",          Physics),
            ("07-camera",        "Câmera: follow + shake",   CameraFollow),
            ("08-particles",     "Partículas",               Particles),
            ("09-audio",         "Música Adaptativa",        Audio),
            ("10-combat",        "Combate (hit/flash)",      Combat),
            ("11-navchaser",     "Pathfinding / NavChaser",  NavChaserExample),
            ("12-ui",            "UI (botão/slider/toggle)", Ui),
            ("13-postprocess",   "Pós-processamento",        PostProcessExample),
        };

        public static Scene? Get(string name)
        {
            foreach (var e in All)
                if (string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase))
                    return e.Build();
            return null;
        }

        // ---------- helpers ----------

        private static GameObject Box(Scene scene, Vector2 pos, Vector2 size, Color color, int sort = 0)
        {
            var o = new GameObject("Box") { SortOrder = sort };
            o.Transform.Position = pos;
            o.AddComponent(new SpriteRenderer { Size = size, Color = color });
            scene.Add(o);
            return o;
        }

        private static void Backdrop(Scene scene, Color color)
        {
            var bg = new GameObject("Fundo") { SortOrder = -1000 };
            bg.AddComponent(new SpriteRenderer { Size = new Vector2(1280, 720), Color = color });
            scene.Add(bg);
        }

        private static void Title(Scene scene, string text)
        {
            var hud = new GameObject("Titulo") { ScreenSpace = true };
            hud.AddComponent(new UiAnchor { Anchor = AnchorPoint.TopLeft, OffsetX = 24, OffsetY = 22 });
            hud.AddComponent(new TextRenderer { Text = text, PixelSize = 3, ScreenSpace = true, Color = Color.White });
            scene.Add(hud);
        }

        private static string Asset(string file)
        {
            foreach (var root in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
            {
                var dir = new DirectoryInfo(root);
                while (dir != null)
                {
                    var candidate = Path.Combine(dir.FullName, "Examples", "assets", file);
                    if (File.Exists(candidate))
                        return candidate;
                    dir = dir.Parent;
                }
            }
            return file;
        }

        // ---------- exemplos ----------

        private static Scene Sprites()
        {
            var s = new Scene { Name = "Sprites e Z-Order" };
            Backdrop(s, new Color(28, 30, 40));
            // Três caixas sobrepostas em ordens diferentes (a de maior SortOrder fica na frente).
            Box(s, new Vector2(-60, -20), new Vector2(180, 180), new Color(220, 70, 70), sort: 0);
            Box(s, new Vector2(20, 30), new Vector2(180, 180), new Color(70, 200, 90), sort: 1);
            Box(s, new Vector2(100, 80), new Vector2(180, 180), new Color(80, 130, 235), sort: 2);
            Title(s, "SPRITES + Z-ORDER");
            return s;
        }

        private static Scene Tween()
        {
            var s = new Scene { Name = "Timeline / Tween" };
            Backdrop(s, new Color(24, 26, 34));
            var o = Box(s, new Vector2(-260, 0), new Vector2(120, 120), new Color(240, 180, 70));
            var anim = new PropertyAnimator { Duration = 2f, Loop = TweenLoop.PingPong, PlayOnStart = true };
            var px = new PropertyTrack { Channel = AnimChannel.PositionX };
            px.Keys.Add(new AnimKey(0f, -260f)); px.Keys.Add(new AnimKey(2f, 260f));
            var rot = new PropertyTrack { Channel = AnimChannel.Rotation };
            rot.Keys.Add(new AnimKey(0f, 0f)); rot.Keys.Add(new AnimKey(2f, 180f));
            var sc = new PropertyTrack { Channel = AnimChannel.ScaleX };
            sc.Keys.Add(new AnimKey(0f, 1f)); sc.Keys.Add(new AnimKey(2f, 1.8f));
            anim.Tracks.Add(px); anim.Tracks.Add(rot); anim.Tracks.Add(sc);
            o.AddComponent(anim);
            Title(s, "TIMELINE / TWEEN");
            return s;
        }

        private static Scene SpriteAnim()
        {
            var s = new Scene { Name = "Animação de Sprite" };
            Backdrop(s, new Color(30, 34, 44));
            var o = new GameObject("Heroi");
            o.Transform.Position = new Vector2(0, 0);
            var anim = new SpriteAnimator { TexturePath = Asset("hero-sheet.png"), Size = new Vector2(260, 260), Fps = 8f };
            var frames = new List<Rectangle>();
            for (int i = 0; i < 4; i++) frames.Add(new Rectangle(i * 48, 0, 48, 48));
            anim.SetFrames(frames);
            anim.AddClip(SpriteClip.Range("walk", 0, 4, 8f, true));
            anim.Play("walk");
            o.AddComponent(anim);
            s.Add(o);
            Title(s, "ANIMACAO DE SPRITE (walk)");
            return s;
        }

        private static Scene Tilemap()
        {
            var s = new Scene { Name = "Tilemap" };
            Backdrop(s, new Color(18, 20, 28));
            var map = new DreamBit.Engine.Tilemap.Tilemap { TileWidth = 32, TileHeight = 32 };
            map.Tilesets.Add(new DreamBit.Engine.Tilemap.Tileset
            {
                FirstGid = 1, Columns = 2, TileCount = 2, TileWidth = 32, TileHeight = 32,
                ImageSource = Asset("tiles.png"), ResolvedImagePath = Asset("tiles.png")
            });
            var layer = map.PaintLayer();
            layer.Name = "Chao";
            for (int x = 0; x < 20; x++)
                for (int y = 0; y < 10; y++)
                {
                    // grama por cima, pedra embaixo, com uma "colina".
                    int top = 6 - (x % 5 == 0 ? 1 : 0);
                    if (y > top) layer.SetTile(x, y, 2);
                    else if (y == top) layer.SetTile(x, y, 1);
                }
            var o = new GameObject("Terreno");
            o.Transform.Position = new Vector2(-320, -160);
            o.AddComponent(new TilemapRenderer { Map = map, Solid = true, Edited = true });
            s.Add(o);
            Title(s, "TILEMAP");
            return s;
        }

        private static Scene Lights()
        {
            var s = new Scene { Name = "Luzes 2D + Sombras" };
            Backdrop(s, new Color(40, 42, 52));
            // Cenário: algumas caixas como oclusores + luzes coloridas.
            Box(s, new Vector2(-120, 40), new Vector2(70, 200), new Color(90, 90, 110));
            Box(s, new Vector2(160, -30), new Vector2(70, 200), new Color(90, 90, 110));

            var amb = new GameObject("Ambiente");
            amb.AddComponent(new AmbientLight { Color = new Color(40, 44, 64) });
            s.Add(amb);

            var l1 = new GameObject("Luz1"); l1.Transform.Position = new Vector2(-250, -120);
            l1.AddComponent(new Light2D { Radius = 360f, Color = new Color(255, 210, 150), Intensity = 1.3f });
            s.Add(l1);
            var l2 = new GameObject("Luz2"); l2.Transform.Position = new Vector2(220, 120);
            l2.AddComponent(new Light2D { Radius = 340f, Color = new Color(120, 200, 255), Intensity = 1.2f });
            s.Add(l2);

            var occ1 = new GameObject("Oclusor1"); occ1.Transform.Position = new Vector2(-120, 40);
            occ1.AddComponent(new ShadowCaster { Width = 70f, Height = 200f }); s.Add(occ1);
            var occ2 = new GameObject("Oclusor2"); occ2.Transform.Position = new Vector2(160, -30);
            occ2.AddComponent(new ShadowCaster { Width = 70f, Height = 200f }); s.Add(occ2);

            Title(s, "LUZES 2D + SOMBRAS");
            return s;
        }

        private static Scene Physics()
        {
            var s = new Scene { Name = "Física + Joints" };
            Backdrop(s, new Color(24, 26, 34));

            // Chão estático + pilha de caixas dinâmicas caindo.
            var floor = new GameObject("Chao"); floor.Transform.Position = new Vector2(-120, 260);
            floor.AddComponent(new SpriteRenderer { Size = new Vector2(420, 40), Color = new Color(70, 74, 90) });
            floor.AddComponent(new Rigidbody2D { Kind = RigidbodyKind.Static, Shape = ColliderShape.Box, Width = 420, Height = 40 });
            s.Add(floor);
            for (int i = 0; i < 4; i++)
            {
                var c = new GameObject("Caixa" + i);
                c.Transform.Position = new Vector2(-120 + (i % 2 == 0 ? -20 : 20), 40 - i * 70);
                c.AddComponent(new SpriteRenderer { Size = new Vector2(60, 60), Color = new Color(150, 110, 70) });
                c.AddComponent(new Rigidbody2D { Kind = RigidbodyKind.Dynamic, Shape = ColliderShape.Box, Width = 60, Height = 60, Density = 1f, Friction = 0.6f });
                s.Add(c);
            }

            // Pêndulo: caixa dinâmica presa por junta a um ponto fixo do mundo (âncora acima dela).
            var bob = new GameObject("Pendulo") { Tag = "bob" };
            bob.Transform.Position = new Vector2(260, 40);
            bob.AddComponent(new SpriteRenderer { Size = new Vector2(70, 70), Color = new Color(230, 120, 200) });
            bob.AddComponent(new Rigidbody2D { Kind = RigidbodyKind.Dynamic, Shape = ColliderShape.Box, Width = 70, Height = 70, Density = 1f });
            bob.AddComponent(new Joint2D { Kind = Joint2DKind.Distance, Anchor = new Vector2(0, -220) }); // preso 220px acima
            s.Add(bob);

            Title(s, "FISICA + JOINTS");
            return s;
        }

        private static Scene CameraFollow()
        {
            var s = new Scene { Name = "Câmera: follow + shake" };
            Backdrop(s, new Color(26, 30, 40));
            // Cenário fixo (postes) para o movimento da câmera ser visível.
            for (int i = -3; i <= 3; i++)
                Box(s, new Vector2(i * 220, 180), new Vector2(40, 220), new Color(70, 80, 100), sort: 0);

            // "Player" que anda de um lado a outro; a câmera segue.
            var player = new GameObject("Player") { Tag = "player" };
            player.Transform.Position = new Vector2(-500, -40);
            player.AddComponent(new SpriteRenderer { Size = new Vector2(90, 90), Color = new Color(240, 200, 80) });
            var walk = new PropertyAnimator { Duration = 4f, Loop = TweenLoop.PingPong, PlayOnStart = true };
            var px = new PropertyTrack { Channel = AnimChannel.PositionX };
            px.Keys.Add(new AnimKey(0f, -500f)); px.Keys.Add(new AnimKey(4f, 500f));
            walk.Tracks.Add(px);
            player.AddComponent(walk);
            s.Add(player);

            // Câmera segue o player e treme quando chega a mensagem "boom".
            var cam = new GameObject("Camera");
            cam.AddComponent(new CameraComponent
            {
                TargetTag = "player", DeadzoneWidth = 60f, DeadzoneHeight = 60f, SmoothTime = 0.15f, Zoom = 1f,
                ShakeOnMessage = "boom", ShakeMessageDuration = 0.4f, ShakeMessageMagnitude = 16f
            });
            s.Add(cam);
            // Timer dispara "boom" a cada 2s.
            var t = new GameObject("Timer");
            t.AddComponent(new TimerComponent { Duration = 2f, Repeat = true, AutoStart = true, SendOnElapsed = "boom" });
            s.Add(t);

            Title(s, "CAMERA: FOLLOW + SHAKE");
            return s;
        }

        private static Scene Particles()
        {
            var s = new Scene { Name = "Partículas" };
            Backdrop(s, new Color(18, 18, 26));
            var o = new GameObject("Emissor");
            o.Transform.Position = new Vector2(0, 120);
            o.AddComponent(new ParticleEmitter
            {
                EmitRate = 90f, Lifetime = 1.6f, Speed = 260f, Spread = 0.7f,
                Size = 14f, EndSize = 2f, GravityY = 240f,
                Color = new Color(255, 200, 90), EndColor = new Color(230, 60, 40)
            });
            s.Add(o);
            Title(s, "PARTICULAS");
            return s;
        }

        private static Scene Audio()
        {
            var s = new Scene { Name = "Música Adaptativa" };
            Backdrop(s, new Color(24, 26, 34));

            var enemy = new GameObject("Inimigo") { Tag = "Inimigo" };
            enemy.Transform.Position = new Vector2(-1400, 0);
            enemy.AddComponent(new SpriteRenderer { Size = new Vector2(140, 140), Color = new Color(220, 70, 70) });
            var move = new PropertyAnimator { Duration = 6f, Loop = TweenLoop.PingPong, PlayOnStart = true };
            var px = new PropertyTrack { Channel = AnimChannel.PositionX, Easing = Scrawlbit.EasingMode.Linear };
            px.Keys.Add(new AnimKey(0f, -1400f)); px.Keys.Add(new AnimKey(6f, 1400f));
            move.Tracks.Add(px);
            enemy.AddComponent(move);
            s.Add(enemy);

            var music = new GameObject("Musica");
            var layered = new LayeredMusic { BaseTrackPath = Asset("base.wav"), BaseVolume = 0.8f, Bus = AudioMixer.Music };
            layered.AddLayer(new MusicLayer { TrackPath = Asset("drums.wav"), Tag = "Inimigo", MinCount = 1, OnlyOnScreen = true, FadeTime = 1f, MaxVolume = 1f });
            music.AddComponent(layered);
            s.Add(music);

            Title(s, "MUSICA ADAPTATIVA (bateria sobe com inimigo na tela)");
            return s;
        }

        private static Scene Combat()
        {
            var s = new Scene { Name = "Combate" };
            Backdrop(s, new Color(28, 24, 28));

            // Alvo: vida + área que recebe golpe (time 1) + piscar ao levar dano.
            var target = new GameObject("Alvo") { Tag = "alvo" };
            target.Transform.Position = new Vector2(80, 0);
            target.AddComponent(new SpriteRenderer { Size = new Vector2(120, 160), Color = new Color(90, 200, 120) });
            target.AddComponent(new Health { Max = 999f, InvulnTime = 0.15f });
            target.AddComponent(new Hurtbox { Team = 1, Width = 120f, Height = 160f });
            target.AddComponent(new SpriteFlash { FlashColor = new Color(255, 90, 90), Duration = 0.12f });
            s.Add(target);

            // Atacante: hitbox (time 0) ativada pela mensagem "hit" (disparada por um timer).
            var atk = new GameObject("Atacante");
            atk.Transform.Position = new Vector2(-80, 0);
            atk.AddComponent(new SpriteRenderer { Size = new Vector2(120, 160), Color = new Color(230, 150, 70) });
            atk.AddComponent(new Hitbox { Team = 0, Damage = 8f, Width = 180f, Height = 120f, Offset = new Vector2(120, 0), ActivateOn = "hit" });
            s.Add(atk);

            var t = new GameObject("Timer");
            t.AddComponent(new TimerComponent { Duration = 0.8f, Repeat = true, AutoStart = true, SendOnElapsed = "hit" });
            s.Add(t);

            Title(s, "COMBATE (hitbox por evento -> flash)");
            return s;
        }

        private static Scene NavChaserExample()
        {
            var s = new Scene { Name = "Pathfinding / NavChaser" };
            Backdrop(s, new Color(20, 26, 30));

            // Alvo que se move; o perseguidor segue (em linha reta, sem tilemap).
            var target = new GameObject("Alvo") { Tag = "player" };
            target.Transform.Position = new Vector2(-300, -120);
            target.AddComponent(new SpriteRenderer { Size = new Vector2(70, 70), Color = new Color(240, 210, 80) });
            var move = new PropertyAnimator { Duration = 3f, Loop = TweenLoop.PingPong, PlayOnStart = true };
            var px = new PropertyTrack { Channel = AnimChannel.PositionX };
            px.Keys.Add(new AnimKey(0f, -300f)); px.Keys.Add(new AnimKey(3f, 300f));
            var py = new PropertyTrack { Channel = AnimChannel.PositionY };
            py.Keys.Add(new AnimKey(0f, -120f)); py.Keys.Add(new AnimKey(3f, 120f));
            move.Tracks.Add(px); move.Tracks.Add(py);
            target.AddComponent(move);
            s.Add(target);

            var chaser = new GameObject("Perseguidor");
            chaser.Transform.Position = new Vector2(250, 150);
            chaser.AddComponent(new SpriteRenderer { Size = new Vector2(70, 70), Color = new Color(230, 90, 90) });
            chaser.AddComponent(new NavChaser { TargetTag = "player", Speed = 160f, RepathInterval = 0.3f });
            s.Add(chaser);

            Title(s, "NAVCHASER (persegue o alvo)");
            return s;
        }

        private static Scene Ui()
        {
            var s = new Scene { Name = "UI" };
            Backdrop(s, new Color(26, 28, 38));

            void Hud(string name, AnchorPoint anchor, float ox, float oy, Action<GameObject> add)
            {
                var o = new GameObject(name) { ScreenSpace = true };
                o.AddComponent(new UiAnchor { Anchor = anchor, OffsetX = ox, OffsetY = oy });
                add(o);
                s.Add(o);
            }

            Hud("Botao", AnchorPoint.Center, -110, -40, o => o.AddComponent(new UiButton { Width = 220, Height = 56, SendOnClick = "play", Normal = new Color(70, 130, 220) }));
            Hud("Rotulo", AnchorPoint.Center, -70, -30, o => o.AddComponent(new TextRenderer { Text = "JOGAR", PixelSize = 3, ScreenSpace = true, Color = Color.White }));
            Hud("Barra", AnchorPoint.Center, -110, 40, o => o.AddComponent(new UiProgressBar { Value = 0.65f, Width = 220, Height = 22, Fill = new Color(210, 90, 90) }));
            Hud("Slider", AnchorPoint.Center, -110, 90, o => o.AddComponent(new UiSlider { Value = 0.7f, Width = 220, Height = 22, BusTarget = AudioMixer.Music }));
            Hud("Toggle", AnchorPoint.Center, -110, 140, o => o.AddComponent(new UiToggle { IsOn = true, Size = 28, SendOnChange = "mute" }));

            Title(s, "UI (botao/barra/slider/toggle/texto)");
            return s;
        }

        private static Scene PostProcessExample()
        {
            var s = new Scene { Name = "Pós-processamento" };
            Backdrop(s, new Color(30, 34, 46));
            Box(s, new Vector2(-160, -20), new Vector2(180, 180), new Color(220, 70, 70), sort: 0);
            Box(s, new Vector2(0, 40), new Vector2(180, 180), new Color(70, 200, 90), sort: 1);
            Box(s, new Vector2(160, -10), new Vector2(180, 180), new Color(80, 130, 235), sort: 2);
            // Pós-processamento em tons de cinza (roda no Player via shader compilado).
            var fx = new GameObject("PostFX");
            fx.AddComponent(new PostProcess { Saturation = 0f });
            s.Add(fx);
            Title(s, "POS-PROCESSAMENTO (grayscale)");
            return s;
        }
    }
}
