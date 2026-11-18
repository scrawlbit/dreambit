using System.IO;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Hosting;
using DreamBit.Engine.Levels;
using DreamBit.Engine.Saving;
using DreamBit.Engine.Serialization;
using Microsoft.Xna.Framework;
using Xunit;
using GameInput = DreamBit.Engine.Input.Input;

namespace DreamBit.Engine.Tests
{
    public class LevelsTests
    {
        private static readonly GameTime Frame = new(System.TimeSpan.Zero, System.TimeSpan.FromSeconds(1.0 / 60));

        private static GameObject Player(string name, Vector2 at, int order = 0)
        {
            var p = new GameObject(name) { Tag = name };
            p.AddComponent(new PlatformerController { Gravity = 0f, HalfWidth = 16, HalfHeight = 24 });
            p.AddComponent(new Persistent { PartyOrder = order });
            p.Transform.Position = at;
            return p;
        }

        [Fact]
        public void Porta_TrocaDeSala_LevaOGrupo_EPosicionaNoSpawn()
        {
            var dir = Path.Combine(Path.GetTempPath(), "dbrooms_" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                // Sala A: Ruber e Aloine perto de uma porta para a sala B.
                var a = new Scene { Name = "A" };
                a.Add(Player("Ruber", new Vector2(0, 0)));
                a.Add(Player("Aloine", new Vector2(-40, 0), 1));
                var door = new GameObject("Porta");
                door.Transform.Position = new Vector2(200, 0);
                door.AddComponent(new RoomDoor { TargetScene = "B.dbscene", TargetSpawn = "entrada", Tag = "Ruber", Size = new Vector2(40, 80) });
                a.Add(door);
                SceneSerializer.Save(a, Path.Combine(dir, "A.dbscene"));

                // Sala B: spawn "entrada" e uma cópia de teste do Ruber (deve ser descartada).
                var b = new Scene { Name = "B" };
                var spawn = new GameObject("Spawn");
                spawn.Transform.Position = new Vector2(1000, 500);
                spawn.AddComponent(new SpawnPoint { Name = "entrada", Facing = 1, PartySpacing = 50 });
                b.Add(spawn);
                b.Add(Player("Ruber", new Vector2(-999, -999)));
                SceneSerializer.Save(b, Path.Combine(dir, "B.dbscene"));

                using var host = new SceneHost();
                host.LoadScene(Path.Combine(dir, "A.dbscene"));
                var ruber = host.Scene.Objects.First(o => o.Name == "Ruber");
                var pc = ruber.Components.OfType<PlatformerController>().Single();
                var ai = new Input.VirtualInput();
                pc.InputOverride = ai;
                ai.SetAxes(1f);
                for (int i = 0; i < 120 && host.Scene.Name == "A"; i++)
                {
                    GameInput.Update();
                    host.Update(Frame);
                }

                Assert.Equal("B", host.Scene.Name);
                Assert.Same(ruber, host.Scene.Objects.Single(o => o.Name == "Ruber")); // o mesmo objeto veio junto
                Assert.Equal(new Vector2(1000, 500), ruber.Transform.Position);
                var aloine = host.Scene.Objects.Single(o => o.Name == "Aloine");
                Assert.Equal(new Vector2(950, 500), aloine.Transform.Position);
                Assert.Equal("B.dbscene", WorldNavigator.CurrentSceneFile);
            }
            finally { Directory.Delete(dir, true); }
        }

        [Fact]
        public void Porta_Trancada_SoAbreComFlag()
        {
            WorldState.Reset();
            var door = new RoomDoor { RequiredFlag = "chave.torre" };
            Assert.True(door.IsLocked);
            WorldState.SetFlag("chave.torre");
            Assert.False(door.IsLocked);
            WorldState.Reset();
        }

        [Fact]
        public void Pickup_LiberaHabilidade_EPersiste()
        {
            WorldState.Reset();
            var scene = new Scene();
            var p = Player("Reanna", Vector2.Zero);
            p.Tag = "Player";
            scene.Add(p);
            var item = new GameObject("Coruja");
            item.AddComponent(new Pickup { UnlockAbilities = "DoubleJump, Glide", PersistKey = "coruja", CounterKey = "almas", Tag = "Player" });
            scene.Add(item);
            scene.StartPlay();
            scene.Update(Frame);

            var pc = p.Components.OfType<PlatformerController>().Single();
            Assert.True(pc.Has(MovementAbilities.DoubleJump | MovementAbilities.Glide));
            Assert.True(WorldState.HasFlag("coruja"));
            Assert.Equal(1, WorldState.GetCounter("almas"));
            Assert.DoesNotContain(item, scene.Objects);
            WorldState.Reset();
        }

        [Fact]
        public void PlacaDePressao_AbrePortao_EnquantoHaAlguemEmCima()
        {
            var scene = new Scene();
            var plate = new GameObject("Placa");
            plate.AddComponent(new Switch { Mode = SwitchMode.PressurePlate, Signal = "porta1", Tag = "Aloine", Size = new Vector2(64, 24) });
            scene.Add(plate);
            var gateObj = new GameObject("Portao");
            gateObj.Transform.Position = new Vector2(500, 0);
            var gate = gateObj.AddComponent(new Gate { OpenWhen = "porta1", Duration = 0.1f, OpenOffset = new Vector2(0, -200) });
            scene.Add(gateObj);
            var aloine = Player("Aloine", new Vector2(1000, 0));
            scene.Add(aloine);
            scene.StartPlay();

            for (int i = 0; i < 10; i++) scene.Update(Frame);
            Assert.False(gate.IsOpen);

            aloine.Transform.Position = Vector2.Zero; // pisa na placa
            for (int i = 0; i < 10; i++) scene.Update(Frame);
            Assert.True(gate.IsOpen);
            Assert.Equal(-200f, gateObj.Transform.Position.Y, 1);

            aloine.Transform.Position = new Vector2(1000, 0); // sai: fecha
            for (int i = 0; i < 10; i++) scene.Update(Frame);
            Assert.False(gate.IsOpen);
            Assert.Equal(0f, gateObj.Transform.Position.Y, 1);
        }

        [Fact]
        public void ZonaDeCamera_LimitaAoRetanguloMenor()
        {
            var scene = new Scene();
            var p = Player("Ruber", new Vector2(100, 100));
            scene.Add(p);
            var camObj = new GameObject("Cam");
            var cam = camObj.AddComponent(new CameraComponent { TargetTag = "Ruber" });
            scene.Add(camObj);
            var big = new GameObject("Grande");
            big.AddComponent(new CameraZone { Size = new Vector2(4000, 4000) });
            scene.Add(big);
            var small = new GameObject("Sala");
            small.Transform.Position = new Vector2(100, 100);
            small.AddComponent(new CameraZone { Size = new Vector2(1600, 1000) });
            scene.Add(small);
            scene.StartPlay();

            cam.DriveCamera(new Rendering.Camera2D(), 1f / 60, 1280, 720); // descobre os alvos
            scene.Update(Frame);
            Assert.Equal((new Vector2(-700, -400), new Vector2(900, 600)), cam.BoundsOverride);

            p.Transform.Position = new Vector2(1500, 1500); // fora da sala, dentro da grande
            cam.DriveCamera(new Rendering.Camera2D(), 1f / 60, 1280, 720);
            scene.Update(Frame);
            Assert.Equal((new Vector2(-2000, -2000), new Vector2(2000, 2000)), cam.BoundsOverride);
        }

        [Fact]
        public void SaveSlots_GravaEListaECarrega()
        {
            var dir = Path.Combine(Path.GetTempPath(), "dbsaves_" + System.Guid.NewGuid().ToString("N"));
            SaveGame.Folder = dir;
            try
            {
                SaveSlots.NewGame(2);
                WorldState.SetFlag("chefe1");
                SaveSlots.PlayTime = System.TimeSpan.FromMinutes(12);
                SaveSlots.Label = "Bosque";
                SaveSlots.Save(2, "Bosque_01.dbscene", "fogueira");

                var info = Assert.Single(SaveSlots.List());
                Assert.Equal(2, info.Slot);
                Assert.Equal("Bosque_01.dbscene", info.Scene);
                Assert.Equal("fogueira", info.Spawn);
                Assert.Equal(12, info.PlayTime.TotalMinutes, 1);
                Assert.Equal("Bosque", info.Label);

                SaveGame.Clear();
                Assert.False(WorldState.HasFlag("chefe1"));
                Assert.Equal("Bosque_01.dbscene", SaveSlots.Load(2));
                Assert.True(WorldState.HasFlag("chefe1"));
                Assert.Equal("fogueira", WorldNavigator.PendingSpawn);
            }
            finally
            {
                SaveGame.Folder = null!;
                WorldNavigator.PendingSpawn = null;
                SaveGame.Clear();
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
            }
        }

        [Fact]
        public void MapRoom_DescobreAoVisitar()
        {
            WorldState.Reset();
            var scene = new Scene();
            var p = Player("Ruber", new Vector2(0, 0));
            scene.Add(p);
            var room = new GameObject("Sala");
            room.AddComponent(new MapRoom { RoomId = "bosque.01", Size = new Vector2(500, 500), Region = "Bosque" });
            scene.Add(room);
            scene.StartPlay();
            Assert.False(WorldMap.IsDiscovered("bosque.01"));
            scene.Update(Frame);
            Assert.True(WorldMap.IsDiscovered("bosque.01"));
            Assert.Single(WorldMap.Rooms(scene));
            WorldState.Reset();
        }
    }
}
