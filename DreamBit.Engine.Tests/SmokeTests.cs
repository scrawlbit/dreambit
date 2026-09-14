using System;
using System.Collections.Generic;
using System.Linq;
using DreamBit.Engine.Animation;
using DreamBit.Engine.Components;
using DreamBit.Engine.Data;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Localization;
using DreamBit.Engine.Pooling;
using DreamBit.Engine.Rendering;
using DreamBit.Engine.Saving;
using DreamBit.Engine.Serialization;
using DreamBit.Engine.Timing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using GameInput = DreamBit.Engine.Input.Input;

namespace DreamBit.Engine.Tests
{
    /// <summary>
    /// Smoke test de integração: monta uma cena usando o conjunto de funcionalidades da engine,
    /// faz round-trip de serialização e roda o laço de play, verificando que tudo interage sem
    /// quebrar e com os resultados esperados.
    /// </summary>
    [TestClass]
    public class SmokeTests
    {
        private static GameTime Frame => new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.016));

        private static Scene BuildScene()
        {
            var scene = new Scene { Name = "Smoke" };

            // Fundo com parallax numa camada baixa.
            var fundo = new GameObject("Fundo") { RenderLayer = -100 };
            fundo.AddComponent(new SpriteRenderer { Size = new Vector2(2000, 600) });
            fundo.AddComponent(new ParallaxLayer { FactorX = 0.3f, FactorY = 0.5f });
            scene.Add(fundo);

            // Chão de tilemap sólido.
            var chao = new GameObject("Chao");
            var map = new Tilemap.Tilemap { TileWidth = 32, TileHeight = 32 };
            var layer = new Tilemap.TileLayer { Name = "Solido" };
            for (int x = -5; x <= 20; x++) layer.SetTile(x, 6, 1);
            map.Layers.Add(layer);
            chao.AddComponent(new TilemapRenderer { Map = map, Solid = true });
            scene.Add(chao);

            // Herói: plataforma + animação (frames explícitos) + controlador de estados + script.
            var heroi = new GameObject("Heroi") { Tag = "Player" };
            heroi.Transform.Position = new Vector2(0, -80);
            heroi.AddComponent(new PlatformerController { Gravity = 900f, HalfHeight = 16f, HalfWidth = 12f, UseKeyboard = false, HorizontalSpeed = 0f });
            var anim = new SpriteAnimator { TexturePath = "hero.png", Fps = 10f, Loop = true };
            anim.SetFrames(new[] { new Rectangle(0, 0, 16, 24), new Rectangle(16, 0, 20, 24) });
            heroi.AddComponent(anim);
            var rig = new SkeletonAnimator(); rig.CurrentClip.Name = "idle";
            var bone = new GameObject("B"); bone.AddComponent(new Bone()); heroi.AddChild(bone);
            heroi.AddComponent(rig); rig.CaptureKeyframe(0f);
            heroi.AddComponent(new AnimatorController { IdleClip = "idle" });
            heroi.AddComponent(new TweenComponent { Channel = TweenChannel.Rotation, From = 0f, To = 10f, Duration = 1f, PlayOnStart = true });
            heroi.AddComponent(new ScriptComponent()); // fonte padrão (gira)
            scene.Add(heroi);

            // Câmera segue o Player.
            var cam = new GameObject("Camera");
            cam.AddComponent(new CameraComponent { TargetTag = "Player", SmoothTime = 0.05f });
            scene.Add(cam);

            // Timer periódico dispara "tick".
            var relogio = new GameObject("Relogio");
            relogio.AddComponent(new TimerComponent { Duration = 0.3f, Repeat = true, SendOnElapsed = "tick" });
            scene.Add(relogio);

            // Painel de HUD ancorado, com layout, texto localizado e botão.
            var hud = new GameObject("HUD") { ScreenSpace = true };
            hud.AddComponent(new UiAnchor { Anchor = AnchorPoint.TopLeft, OffsetX = 200, OffsetY = 200 });
            hud.AddComponent(new UiLayout { Direction = LayoutDirection.Vertical, Spacing = 10f });
            var titulo = new GameObject("Titulo");
            titulo.AddComponent(new TextRenderer { LocKey = "menu.play", Text = "PLAY", ScreenSpace = false });
            hud.AddChild(titulo);
            var botao = new GameObject("Botao");
            botao.AddComponent(new UiButton { Width = 160, Height = 48, SendOnClick = "start" });
            hud.AddChild(botao);
            scene.Add(hud);

            // Caixa com física real (corpo rígido) caindo.
            var caixa = new GameObject("Caixa");
            caixa.Transform.Position = new Vector2(50, -60);
            caixa.AddComponent(new Rigidbody2D { Kind = RigidbodyKind.Dynamic, Shape = ColliderShape.Box, Width = 24, Height = 24 });
            scene.Add(caixa);

            // Ouvinte reage ao clique escondendo o HUD.
            var ouvinte = new GameObject("Ouvinte");
            ouvinte.AddComponent(new MessageListener { Message = "start", Reaction = MessageReaction.None });
            scene.Add(ouvinte);

            return scene;
        }

        [TestMethod]
        public void CenaCompleta_RoundTrip_Play_Interacoes()
        {
            Screen.Set(800, 600);
            Screen.CameraPosition = Vector2.Zero;
            Localizer.Clear();
            Localizer.Language = "pt";
            Localizer.Load("pt", new Dictionary<string, string> { ["menu.play"] = "JOGAR" });
            Scheduler.Clear();
            SaveGame.Clear();
            DataCatalog.Clear();

            // 1) Serialização round-trip preserva os componentes.
            var original = BuildScene();
            var scene = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(original));

            GameObject Find(string name) => scene.VisibleInDrawOrder().First(o => o.Name == name);
            var heroi = scene.VisibleInDrawOrder().First(o => o.Tag == "Player");
            Assert.IsTrue(heroi.Components.OfType<SpriteAnimator>().Single().Frames.Count == 2, "frames explícitos sobrevivem");
            Assert.IsTrue(scene.VisibleInDrawOrder().Any(o => o.Components.OfType<UiButton>().Any()), "botão sobrevive");

            // 2) Sistemas de runtime (APIs para scripts).
            SaveGame.SetInt("fase", 1);
            DataCatalog.LoadMapFromJson<Dictionary<string, object>>("{}");
            int agendado = 0;
            bool correr = false;
            var sm = new StateMachine().AddState("idle").AddState("run")
                .AddTransition("idle", "run", () => correr);
            sm.Start("idle");
            var pool = new ObjectPool(scene, new GameObject("Bala"), prewarm: 2);

            // 3) Play.
            int ticks = 0;
            scene.MessageSent += m => { if (m.Name == "tick") ticks++; };
            bool startRecebido = false;
            scene.MessageSent += m => { if (m.Name == "start") startRecebido = true; };

            scene.StartPlay(); // limpa o Scheduler; agende depois dele
            Scheduler.After(0.2f, () => agendado++);
            var camera = new Camera2D();
            var camComp = Find("Camera").Components.OfType<CameraComponent>().Single();

            for (int i = 0; i < 90; i++)
            {
                GameInput.SetPointer(new Vector2(-1, -1), false); // ponteiro fora
                sm.Update(0.016f);
                scene.Update(Frame); // já avança o Scheduler
                camComp.DriveCamera(camera, 0.016f, 800, 600);
                Screen.CameraPosition = camera.Position;
                if (i == 10) correr = true; // aciona transição da máquina de estados
            }

            // 4) Verificações de interação.
            var platform = heroi.Components.OfType<PlatformerController>().Single();
            Assert.IsTrue(platform.Grounded, "herói pousou sobre o tilemap sólido");
            Assert.AreNotEqual(0f, camera.Position.Y, "câmera seguiu o herói (caiu no Y)");
            Assert.IsTrue(ticks >= 3, $"timer repetiu (ticks={ticks})");
            Assert.AreEqual(1, agendado, "callback agendado disparou uma vez");
            Assert.AreEqual("run", sm.Current, "máquina de estados transicionou");

            var fundo = Find("Fundo");
            Assert.IsTrue(fundo.Components.OfType<ParallaxLayer>().Any());
            Assert.AreNotEqual(0f, fundo.Transform.Position.Y, "parallax deslocou o fundo conforme a câmera (Y)");

            var titulo = scene.VisibleInDrawOrder().First(o => o.Name == "Titulo");
            Assert.AreEqual("JOGAR", titulo.Components.OfType<TextRenderer>().Single().DisplayText, "texto localizado");

            var caixa = scene.VisibleInDrawOrder().First(o => o.Name == "Caixa");
            Assert.IsTrue(caixa.Transform.Position.Y > -60f + 10f, "corpo rígido caiu pela física");

            // 5) Clica no botão (posição de tela calculada) e confirma a mensagem.
            var botao = scene.VisibleInDrawOrder().First(o => o.Name == "Botao").Components.OfType<UiButton>().Single();
            var r = botao.ScreenRect();
            var centro = new Vector2(r.X + r.Width / 2f, r.Y + r.Height / 2f);
            GameInput.SetPointer(centro, true);  scene.Update(Frame); // pressiona
            GameInput.SetPointer(centro, false); scene.Update(Frame); // solta => clique
            Assert.IsTrue(startRecebido, "clique no botão disparou a mensagem 'start'");

            // 6) Object pool.
            var b = pool.Get();
            Assert.AreEqual(1, pool.ActiveCount);
            Assert.IsTrue(pool.Return(b));

            GameInput.ClearPointerOverride();
        }
    }
}
