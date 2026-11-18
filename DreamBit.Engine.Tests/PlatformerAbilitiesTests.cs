using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Input;
using DreamBit.Engine.Saving;
using Microsoft.Xna.Framework;
using Xunit;
using GameInput = DreamBit.Engine.Input.Input;

namespace DreamBit.Engine.Tests
{
    public class PlatformerAbilitiesTests
    {
        private static readonly GameTime Frame = new(System.TimeSpan.Zero, System.TimeSpan.FromSeconds(1.0 / 60));

        private sealed class World
        {
            public Scene Scene = new();
            public GameObject Player = new("Player") { Tag = "Player" };
            public PlatformerController Pc;
            public VirtualInput In = new();

            public World(MovementAbilities abilities = MovementAbilities.None)
            {
                Pc = Player.AddComponent(new PlatformerController { HalfWidth = 16, HalfHeight = 24, CrouchHalfHeight = 12 });
                Pc.Abilities = abilities | MovementAbilities.Crouch | MovementAbilities.DropThrough;
                Pc.InputOverride = In;
                Scene.Add(Player);
                Solid("Chao", new Vector2(0, 100), new Vector2(4000, 40)); // topo em y = 80
                Player.Transform.Position = new Vector2(0, 80 - 24);
                Scene.StartPlay();
            }

            public GameObject Solid(string name, Vector2 center, Vector2 size)
            {
                var o = new GameObject(name);
                o.Transform.Position = center;
                o.AddComponent(new BoxCollider { Size = size });
                Scene.Add(o);
                return o;
            }

            public void Step(int frames = 1)
            {
                for (int i = 0; i < frames; i++)
                {
                    GameInput.Update();
                    Scene.Update(Frame);
                }
            }

            public void Press(string action) { In.Set(action, true); Step(); }
            public void Release(string action) { In.Set(action, false); }
            public Vector2 Pos => Player.Transform.Position;
        }

        [Fact]
        public void Pousa_NoChao()
        {
            var w = new World();
            w.Step(5);
            Assert.True(w.Pc.Grounded);
            Assert.Equal(56f, w.Pos.Y, 1);
        }

        [Fact]
        public void PuloVariavel_SoltarCedoPulaMenos()
        {
            var alto = new World();
            alto.Step(3);
            alto.In.Set("Jump", true);
            float topoAlto = float.MaxValue;
            for (int i = 0; i < 60; i++) { alto.Step(); topoAlto = System.Math.Min(topoAlto, alto.Pos.Y); }

            var baixo = new World();
            baixo.Step(3);
            baixo.Press("Jump");
            baixo.Step(2);
            baixo.Release("Jump");
            float topoBaixo = float.MaxValue;
            for (int i = 0; i < 60; i++) { baixo.Step(); topoBaixo = System.Math.Min(topoBaixo, baixo.Pos.Y); }

            Assert.True(topoBaixo > topoAlto + 40f, $"curto {topoBaixo} vs longo {topoAlto}");
        }

        [Fact]
        public void CoyoteTime_PermitePularLogoAposSairDaBorda()
        {
            var w = new World();
            w.Scene.Remove(w.Scene.Objects.First(o => o.Name == "Chao"));
            w.Solid("Borda", new Vector2(-1000, 100), new Vector2(2000, 40)); // termina em x = 0
            w.Player.Transform.Position = new Vector2(-30, 56);
            w.Step(3);
            Assert.True(w.Pc.Grounded);
            w.In.SetAxes(1f);
            int guard = 0;
            while (w.Pc.Grounded && guard++ < 60) w.Step();
            Assert.False(w.Pc.Grounded);
            w.In.SetAxes(0f);
            w.Press("Jump");
            Assert.True(w.Pc.Velocity.Y < 0f, "deveria pular no coyote time");
        }

        [Fact]
        public void PuloDuplo_SoComHabilidade()
        {
            var sem = new World();
            sem.Step(3); sem.Press("Jump"); sem.Release("Jump"); sem.Step(20);
            float vy = sem.Pc.Velocity.Y;
            sem.Press("Jump");
            Assert.True(sem.Pc.Velocity.Y >= vy, "sem a habilidade não pode pular no ar");

            var com = new World(MovementAbilities.DoubleJump);
            com.Step(3); com.Press("Jump"); com.Release("Jump"); com.Step(20);
            com.Press("Jump");
            Assert.True(com.Pc.Velocity.Y < -400f);
        }

        [Fact]
        public void Dash_QuebraParedeComBreakWalls()
        {
            var w = new World(MovementAbilities.Dash | MovementAbilities.BreakWalls);
            var parede = w.Solid("Parede", new Vector2(120, 40), new Vector2(40, 80));
            parede.AddComponent(new Breakable { BreakBy = Breakable.ByDash, BurstParticles = 0 });
            w.Step(3);
            w.In.SetAxes(1f);
            w.Press("Dash");
            w.Step(10);
            Assert.DoesNotContain(parede, w.Scene.Objects);
            Assert.True(w.Pos.X > 100f);
        }

        [Fact]
        public void PancadaNoChao_QuebraPisoFragil()
        {
            var w = new World(MovementAbilities.GroundPound | MovementAbilities.BreakFloors);
            var piso = w.Solid("Piso", new Vector2(300, -100), new Vector2(200, 20)); // topo em -110
            piso.AddComponent(new Breakable { BreakBy = Breakable.ByGroundPound, BurstParticles = 0 });
            w.Player.Transform.Position = new Vector2(300, -300);
            w.Step(2);
            w.In.Set("MoveDown", true);
            w.In.SetAxes(0f, 1f);
            w.Press("Attack");
            Assert.Equal(PlatformerState.GroundPound, w.Pc.State);
            w.Step(40);
            w.In.Set("MoveDown", false);
            w.In.SetAxes(0f, 0f);
            w.Step(3);
            Assert.DoesNotContain(piso, w.Scene.Objects);
            Assert.True(w.Pc.Grounded);
            Assert.Equal(56f, w.Pos.Y, 1);
        }

        [Fact]
        public void Agachar_NaoLevantaSobTetoBaixo()
        {
            var w = new World();
            w.Step(3);
            w.In.SetAxes(0f, 1f);
            w.Step(2);
            Assert.Equal(PlatformerState.Crouch, w.Pc.State);
            Assert.Equal(12f, w.Pc.CurrentHalfHeight);
            Assert.Equal(68f, w.Pos.Y, 1); // pés continuam em 80
            w.Solid("Teto", new Vector2(0, 40), new Vector2(200, 20)); // fundo em 50: cabe agachado (56..80), não em pé
            w.In.SetAxes(0f, 0f);
            w.Step(3);
            Assert.Equal(PlatformerState.Crouch, w.Pc.State);
        }

        [Fact]
        public void AgarrarBorda_ESubir()
        {
            var w = new World(MovementAbilities.LedgeGrab);
            // Bloco à direita: face esquerda em x = 100, topo em y = -100.
            w.Solid("Bloco", new Vector2(150, 0), new Vector2(100, 200));
            w.Player.Transform.Position = new Vector2(84, -85); // cabeça em -109, caindo junto à parede
            w.In.SetAxes(1f);
            int guard = 0;
            while (w.Pc.State != PlatformerState.LedgeHang && guard++ < 30) w.Step();
            Assert.Equal(PlatformerState.LedgeHang, w.Pc.State);
            var hang = w.Pos;
            w.Step(5);
            Assert.Equal(hang, w.Pos); // fica parado pendurado
            w.In.SetAxes(1f, -1f);
            w.Step(30);
            Assert.True(w.Pos.X > 100f, $"deveria estar em cima do bloco: {w.Pos}");
            Assert.True(w.Pos.Y < -100f);
        }

        [Fact]
        public void PularNaParede_EmpurraParaOLadoOposto()
        {
            var w = new World(MovementAbilities.WallJump);
            w.Solid("Parede", new Vector2(100, -300), new Vector2(40, 600)); // face esquerda em 80
            w.Player.Transform.Position = new Vector2(63, -200);
            w.In.SetAxes(1f);
            w.Step(3);
            Assert.Equal(PlatformerState.WallSlide, w.Pc.State);
            w.Press("Jump");
            Assert.True(w.Pc.Velocity.X < 0f && w.Pc.Velocity.Y < 0f);
        }

        [Fact]
        public void Empurrar_MoveOCaixote()
        {
            var w = new World(MovementAbilities.Push);
            var caixa = w.Solid("Caixa", new Vector2(80, 56), new Vector2(48, 48));
            caixa.AddComponent(new Pushable());
            w.Step(3);
            float antes = caixa.Transform.Position.X;
            w.In.SetAxes(1f);
            w.Step(30);
            Assert.True(caixa.Transform.Position.X > antes + 10f);
        }

        [Fact]
        public void BaixoMaisPulo_AtravessaLedge()
        {
            var w = new World();
            var ledge = new Ledge { Name = "L", OneWay = true };
            ledge.SetPoints(new[] { new Vector2(-200, -100), new Vector2(200, -100) });
            w.Scene.AddLedge(ledge);
            w.Player.Transform.Position = new Vector2(0, -130);
            w.Step(30);
            Assert.True(w.Pc.Grounded);
            Assert.Equal(-124f, w.Pos.Y, 1);
            w.In.SetAxes(0f, 1f);
            w.Press("Jump");
            w.In.SetAxes(0f, 0f);
            w.Release("Jump");
            w.Step(40);
            Assert.Equal(56f, w.Pos.Y, 1); // caiu até o chão
        }

        [Fact]
        public void PlataformaMovel_CarregaOJogador()
        {
            var w = new World();
            var plat = w.Solid("Plataforma", new Vector2(0, -50), new Vector2(200, 20)); // topo em -60
            w.Player.Transform.Position = new Vector2(0, -90);
            w.Step(30);
            Assert.True(w.Pc.Grounded);
            float x0 = w.Pos.X;
            for (int i = 0; i < 10; i++)
            {
                plat.Transform.Position += new Vector2(5, 0);
                w.Step();
            }
            Assert.Equal(x0 + 50f, w.Pos.X, 0); // acompanha todo o deslocamento da plataforma
        }

        [Fact]
        public void Breakable_PersistKey_ContinuaQuebrado()
        {
            WorldState.Reset();
            var scene = new Scene();
            var obj = new GameObject("Parede");
            obj.AddComponent(new BoxCollider());
            var b = obj.AddComponent(new Breakable { PersistKey = "parede.1", BurstParticles = 0, DestroyOnBreak = false });
            scene.Add(obj);
            scene.StartPlay();
            Assert.True(b.Hit("Dash"));
            Assert.True(WorldState.HasFlag("parede.1"));

            var again = new Scene();
            var obj2 = new GameObject("Parede");
            var col2 = obj2.AddComponent(new BoxCollider());
            var b2 = obj2.AddComponent(new Breakable { PersistKey = "parede.1" });
            again.Add(obj2);
            again.StartPlay();
            Assert.True(b2.IsBroken);
            Assert.False(col2.Enabled);
            WorldState.Reset();
        }

        [Fact]
        public void Habilidades_SaoSalvasNaCena()
        {
            var scene = new Scene();
            var o = new GameObject("p");
            o.AddComponent(new PlatformerController { Abilities = MovementAbilities.Dash | MovementAbilities.Glide, AirJumps = 2, CoyoteTime = 0.2f });
            scene.Add(o);
            var again = Serialization.SceneSerializer.LoadFromString(Serialization.SceneSerializer.SaveToString(scene));
            var pc = again.Objects[0].Components.OfType<PlatformerController>().Single();
            Assert.Equal(MovementAbilities.Dash | MovementAbilities.Glide, pc.Abilities);
            Assert.Equal(2, pc.AirJumps);
            Assert.Equal(0.2f, pc.CoyoteTime);
        }
    }
}
