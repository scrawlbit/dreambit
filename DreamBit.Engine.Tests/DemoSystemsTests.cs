using System;
using System.Linq;
using DreamBit.Engine.Audio;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Xunit;
using Microsoft.Xna.Framework;
using GameInput = DreamBit.Engine.Input.Input;

namespace DreamBit.Engine.Tests
{
    public class DemoSystemsTests
    {
        private static GameTime Frame(double s) => new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(s));

        private static GameObject Player(Scene scene, Vector2 pos)
        {
            var p = new GameObject("Player") { Tag = "player" };
            p.Transform.Position = pos;
            scene.Add(p);
            return p;
        }

        [Fact]
        public void NavChaser_SemTilemap_PersegueEmLinhaReta()
        {
            var scene = new Scene();
            Player(scene, new Vector2(400, 0));
            var enemy = new GameObject("Enemy");
            enemy.Transform.Position = new Vector2(0, 0);
            enemy.AddComponent(new NavChaser { TargetTag = "player", Speed = 200f });
            scene.Add(enemy);

            scene.StartPlay();
            GameInput.SetPointer(new Vector2(-1, -1), false);
            for (int i = 0; i < 60; i++) scene.Update(Frame(0.05));

            Assert.True(enemy.Transform.Position.X > 350f, $"o inimigo se aproximou do alvo (x={enemy.Transform.Position.X})");
            GameInput.ClearPointerOverride();
        }

        [Fact]
        public void NavChaser_ComParede_DesviaPeloVaoUsandoAStar()
        {
            var scene = new Scene();
            // Sala com borda sólida e uma parede interna vertical com um vão embaixo.
            var map = new Tilemap.Tilemap { TileWidth = 32, TileHeight = 32 };
            var layer = map.PaintLayer();
            for (int x = 0; x <= 12; x++)
                for (int y = 0; y <= 6; y++)
                    if (x == 0 || x == 12 || y == 0 || y == 6)
                        layer.SetTile(x, y, 1);
            for (int y = 1; y <= 4; y++) // parede interna em x=6, vão em y=5
                layer.SetTile(6, y, 1);

            var mapObj = new GameObject("Map");
            mapObj.AddComponent(new TilemapRenderer { Map = map, Solid = true });
            scene.Add(mapObj);

            Player(scene, new Vector2(10 * 32 + 16, 3 * 32 + 16));
            var enemy = new GameObject("Enemy");
            enemy.Transform.Position = new Vector2(2 * 32 + 16, 3 * 32 + 16);
            enemy.AddComponent(new NavChaser { TargetTag = "player", Speed = 260f, RepathInterval = 0.3f });
            scene.Add(enemy);

            scene.StartPlay();
            GameInput.SetPointer(new Vector2(-1, -1), false);
            scene.Update(Frame(0.05)); // primeiro repath

            var chaser = enemy.Components.OfType<NavChaser>().Single();
            Assert.True(chaser.CurrentPath.Count > 1, "achou um caminho com desvio (A*)");

            for (int i = 0; i < 200; i++) scene.Update(Frame(0.05));
            Assert.True(enemy.Transform.Position.X > 6 * 32, $"cruzou a parede interna pelo vão (x={enemy.Transform.Position.X})");
            GameInput.ClearPointerOverride();
        }

        [Fact]
        public void AudioEspacial_AtenuaEPanoramica()
        {
            var listener = new Vector2(100, 100);
            var (atNear, panNear) = AudioSpatial.Compute(listener, listener, 500f);
            Assert.Equal(1f, atNear, 0.001f);
            Assert.Equal(0f, panNear, 0.001f);

            var (atRight, panRight) = AudioSpatial.Compute(new Vector2(350, 100), listener, 500f);
            Assert.True(panRight > 0f, "fonte à direita: pan positivo");
            Assert.True(atRight > 0f && atRight < 1f, "atenua com a distância");

            var (atFar, _) = AudioSpatial.Compute(new Vector2(1000, 100), listener, 500f);
            Assert.Equal(0f, atFar, 0.001f);
        }

        [Fact]
        public void AudioListener_DefineposicaoNoPlay()
        {
            AudioListener.Clear();
            var scene = new Scene();
            var o = new GameObject("Ouvinte");
            o.Transform.Position = new Vector2(42, 24);
            o.AddComponent(new AudioListener());
            scene.Add(o);
            scene.StartPlay();
            Assert.Equal(new Vector2(42, 24), AudioListener.Position);
        }

        [Fact]
        public void Serializacao_RoundTrip_NavChaser_Listener_AudioEspacial()
        {
            var scene = new Scene();
            var o = new GameObject("E");
            o.AddComponent(new NavChaser { TargetTag = "hero", Speed = 175f, RepathInterval = 0.25f, AllowDiagonal = false });
            o.AddComponent(new AudioListener());
            o.AddComponent(new AudioSource { Spatial = true, MaxDistance = 320f, Bus = AudioMixer.Sfx });
            scene.Add(o);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            var e = loaded.Objects.First();
            var chaser = e.Components.OfType<NavChaser>().Single();
            Assert.Equal("hero", chaser.TargetTag);
            Assert.Equal(175f, chaser.Speed);
            Assert.False(chaser.AllowDiagonal);
            Assert.Single(e.Components.OfType<AudioListener>());
            var audio = e.Components.OfType<AudioSource>().Single();
            Assert.True(audio.Spatial);
            Assert.Equal(320f, audio.MaxDistance);
        }
    }
}
