using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Input;
using DreamBit.Engine.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using Xunit;
using GameInput = DreamBit.Engine.Input.Input;

namespace DreamBit.Engine.Tests
{
    public class PlayerInputTests
    {
        private static GameTime Frame => new(System.TimeSpan.Zero, System.TimeSpan.FromSeconds(1.0 / 60));

        [Fact]
        public void VirtualInput_DetectaBordaPorFrame()
        {
            var v = new VirtualInput();
            GameInput.Update();
            v.Set("Jump", true);
            Assert.True(v.IsDown("Jump"));
            Assert.True(v.JustPressed("Jump"));

            GameInput.Update(); // próximo frame, ainda segurando
            Assert.True(v.IsDown("Jump"));
            Assert.False(v.JustPressed("Jump"));

            GameInput.Update();
            v.Set("Jump", false);
            Assert.True(v.JustReleased("Jump"));
        }

        [Fact]
        public void VirtualInput_EixosEAcoes()
        {
            var v = new VirtualInput();
            v.Set("MoveLeft", true);
            Assert.Equal(-1f, v.Horizontal());
            v.SetAxes(0.5f, -0.25f);
            Assert.Equal(0.5f, v.Horizontal());
            Assert.Equal(-0.25f, v.Vertical());
        }

        [Fact]
        public void Coop_DivideTeclado()
        {
            GameInput.Players.ConfigureCoop(sharedKeyboard: true);
            try
            {
                Assert.Equal(KeyboardScheme.Left, GameInput.Players[0].Scheme);
                Assert.Equal(KeyboardScheme.Right, GameInput.Players[1].Scheme);
                Assert.Contains(Keys.A, GameInput.Players[0].KeysFor("MoveLeft"));
                Assert.DoesNotContain(Keys.Left, GameInput.Players[0].KeysFor("MoveLeft"));
                Assert.Contains(Keys.Left, GameInput.Players[1].KeysFor("MoveLeft"));
                Assert.Equal(1, GameInput.Players[1].Gamepad);
            }
            finally { GameInput.Players.ConfigureSinglePlayer(); }
        }

        [Fact]
        public void Rebind_PorJogador()
        {
            var p = new PlayerInput(2, KeyboardScheme.Full, 2);
            p.Map("Jump", new[] { Keys.U }, new[] { Buttons.B });
            Assert.Equal(new[] { Keys.U }, p.KeysFor("Jump"));
            Assert.Equal(new[] { Buttons.B }, p.ButtonsFor("Jump"));
        }

        [Fact]
        public void Platformer_UsaFonteDeInputOverride()
        {
            var scene = new Scene();
            var obj = new GameObject("p");
            var pc = obj.AddComponent(new PlatformerController { Gravity = 0f, MoveSpeed = 100f });
            scene.Add(obj);
            var ai = new VirtualInput();
            pc.InputOverride = ai;
            ai.SetAxes(1f);
            GameInput.Update();
            scene.Update(Frame);
            Assert.True(obj.Transform.Position.X > 1f);
        }

        [Fact]
        public void PlayerIndex_ESalvoNaCena()
        {
            var scene = new Scene();
            var obj = new GameObject("p");
            obj.AddComponent(new PlatformerController { PlayerIndex = 1 });
            obj.AddComponent(new TopDownController { PlayerIndex = 0 });
            scene.Add(obj);
            var again = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            Assert.Equal(1, Assert.Single(again.Objects[0].Components.OfType<PlatformerController>()).PlayerIndex);
            Assert.Equal(0, Assert.Single(again.Objects[0].Components.OfType<TopDownController>()).PlayerIndex);
        }
    }
}
