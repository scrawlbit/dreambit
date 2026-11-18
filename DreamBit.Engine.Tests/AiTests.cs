using System.Linq;
using DreamBit.Engine.AI;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;
using Xunit;
using GameInput = DreamBit.Engine.Input.Input;

namespace DreamBit.Engine.Tests
{
    public class AiTests
    {
        private static readonly GameTime Frame = new(System.TimeSpan.Zero, System.TimeSpan.FromSeconds(1.0 / 60));

        private static Scene World(out GameObject floor)
        {
            var s = new Scene();
            floor = new GameObject("Chao");
            floor.Transform.Position = new Vector2(0, 100);
            floor.AddComponent(new BoxCollider { Size = new Vector2(6000, 40) }); // topo em 80
            s.Add(floor);
            return s;
        }

        private static GameObject Char(Scene s, string name, float x, float y = 56)
        {
            var o = new GameObject(name) { Tag = name };
            o.Transform.Position = new Vector2(x, y);
            o.AddComponent(new PlatformerController { HalfWidth = 16, HalfHeight = 24 });
            s.Add(o);
            return o;
        }

        private static void Run(Scene s, int frames)
        {
            for (int i = 0; i < frames; i++) { GameInput.Update(); s.Update(Frame); }
        }

        [Fact]
        public void Seguidor_AndaAteOLider_EPara()
        {
            var s = World(out _);
            Char(s, "Ruber", 600);
            var aloine = Char(s, "Aloine", 0);
            aloine.AddComponent(new PlatformerFollower { Leader = "Ruber", FollowDistance = 100, StopDistance = 60 });
            s.StartPlay();
            Run(s, 180);
            float gap = 600 - aloine.Transform.Position.X;
            Assert.InRange(gap, 30f, 110f);
        }

        [Fact]
        public void Seguidor_PulaDegrauQuandoOLiderEstaEmCima()
        {
            var s = World(out _);
            var degrau = new GameObject("Degrau");
            degrau.Transform.Position = new Vector2(400, 50);
            degrau.AddComponent(new BoxCollider { Size = new Vector2(400, 60) }); // topo em 20, face esquerda em 200
            s.Add(degrau);
            Char(s, "Ruber", 450, 20 - 24);
            var aloine = Char(s, "Aloine", 0);
            aloine.AddComponent(new PlatformerFollower { Leader = "Ruber" });
            s.StartPlay();
            Run(s, 300);
            Assert.True(aloine.Transform.Position.Y < 0f, $"deveria ter subido: {aloine.Transform.Position}");
            Assert.True(aloine.Transform.Position.X > 200f);
        }

        [Fact]
        public void Seguidor_TeleportaQuandoMuitoLonge()
        {
            var s = World(out _);
            Char(s, "Ruber", 3000);
            var aloine = Char(s, "Aloine", 0);
            aloine.AddComponent(new PlatformerFollower { Leader = "Ruber", TeleportDistance = 900 });
            s.StartPlay();
            Run(s, 2);
            Assert.True(aloine.Transform.Position.X > 2800f);
        }

        [Fact]
        public void Grupo_TrocaDePersonagem_EModoCoop()
        {
            var s = World(out _);
            var ruber = Char(s, "Ruber", 0);
            var aloine = Char(s, "Aloine", -60);
            var mgr = new GameObject("Grupo");
            var party = mgr.AddComponent(new PartySwitcher { Members = "Ruber,Aloine", AllowDropIn = false });
            s.Add(mgr);
            s.StartPlay();

            var pcR = ruber.Components.OfType<PlatformerController>().Single();
            var pcA = aloine.Components.OfType<PlatformerController>().Single();
            Run(s, 1);
            Assert.Equal(0, pcR.PlayerIndex);
            Assert.True(aloine.Components.OfType<PlatformerFollower>().Single().Active);
            Assert.NotNull(pcA.InputOverride);

            party.SwapTo(1);
            Run(s, 1);
            Assert.Same(aloine, party.ActiveMember);
            Assert.Equal(0, pcA.PlayerIndex);
            Assert.Null(pcA.InputOverride);
            Assert.True(ruber.Components.OfType<PlatformerFollower>().Single().Active);

            party.SetCoop(true);
            Run(s, 1);
            Assert.Equal(0, pcA.PlayerIndex);
            Assert.Equal(1, pcR.PlayerIndex);
            Assert.False(ruber.Components.OfType<PlatformerFollower>().Single().Active);
            Assert.Null(pcR.InputOverride);
            party.SetCoop(false);
        }

        [Fact]
        public void Patrulha_ViraNaBordaSemCair()
        {
            var s = new Scene();
            var plat = new GameObject("Plataforma");
            plat.AddComponent(new BoxCollider { Size = new Vector2(400, 40) }); // de -200 a 200, topo em -20
            s.Add(plat);
            var e = Char(s, "Goblin", 0, -44);
            e.AddComponent(new PatrolEnemy { ChaseTarget = "", TurnPause = 0f, PatrolSpeed = 1f });
            s.StartPlay();
            float minX = 0, maxX = 0;
            for (int i = 0; i < 400; i++)
            {
                Run(s, 1);
                minX = System.Math.Min(minX, e.Transform.Position.X);
                maxX = System.Math.Max(maxX, e.Transform.Position.X);
            }
            Assert.True(e.Transform.Position.Y < 0f, "não pode cair da plataforma");
            Assert.True(maxX > 150f && minX < -150f, $"deveria ir e voltar: {minX}..{maxX}");
        }

        [Fact]
        public void Patrulha_PersegueAlvoNoAlcance()
        {
            var s = World(out _);
            Char(s, "Ruber", 250);
            var e = Char(s, "Goblin", 0);
            var patrol = e.AddComponent(new PatrolEnemy { ChaseTarget = "Ruber", ChaseRange = 400 });
            s.StartPlay();
            Run(s, 30);
            Assert.NotNull(patrol.Target);
            Assert.True(e.Transform.Position.X > 50f);
        }

        [Fact]
        public void Projetil_DaDanoEmOutroTime_EQueimaVinhas()
        {
            var s = new Scene();
            var alvo = new GameObject("Inimigo");
            alvo.Transform.Position = new Vector2(200, 0);
            var hp = alvo.AddComponent(new Health { Max = 100 });
            alvo.AddComponent(new Hurtbox { Width = 40, Height = 40, Team = 1 });
            s.Add(alvo);
            var vinhas = new GameObject("Vinhas");
            vinhas.Transform.Position = new Vector2(-200, 0);
            vinhas.AddComponent(new BoxCollider { Size = new Vector2(40, 200) });
            vinhas.AddComponent(new Breakable { BreakBy = "Fire", BurstParticles = 0 });
            s.Add(vinhas);
            s.StartPlay();

            Projectile.Spawn(s, Vector2.Zero, new Vector2(600, 0), team: 0, damage: 30);
            Projectile.Spawn(s, Vector2.Zero, new Vector2(-600, 0), team: 0, breakKind: "Fire");
            Run(s, 40);

            Assert.Equal(70f, hp.Current);
            Assert.DoesNotContain(vinhas, s.Objects);
            Assert.DoesNotContain(s.Objects, o => o.Components.OfType<Projectile>().Any());
        }
    }
}
