using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DreamBit.Engine.Input
{
    /// <summary>
    /// Input mapeável por "ações" (ex.: "MoveLeft", "Jump") em vez de teclas fixas —
    /// equivalente ao InputMap do Godot / Input System do Unity. Suporta teclado e
    /// gamepad. O host (Player) chama <see cref="Update"/> uma vez por frame.
    /// </summary>
    public static class Input
    {
        private sealed record Binding(Keys[] Keys, Buttons[] Buttons);

        private static readonly Dictionary<string, Binding> _map = new()
        {
            ["MoveLeft"] = new(new[] { Keys.Left, Keys.A }, new[] { Buttons.DPadLeft, Buttons.LeftThumbstickLeft }),
            ["MoveRight"] = new(new[] { Keys.Right, Keys.D }, new[] { Buttons.DPadRight, Buttons.LeftThumbstickRight }),
            ["MoveUp"] = new(new[] { Keys.Up, Keys.W }, new[] { Buttons.DPadUp }),
            ["MoveDown"] = new(new[] { Keys.Down, Keys.S }, new[] { Buttons.DPadDown }),
            ["Jump"] = new(new[] { Keys.Space, Keys.W, Keys.Up }, new[] { Buttons.A }),
            ["Action"] = new(new[] { Keys.Enter, Keys.E }, new[] { Buttons.X }),
        };

        private static KeyboardState _kbNow, _kbPrev;
        private static GamePadState _padNow, _padPrev;

        /// <summary>Snapshot do frame (chame uma vez por frame antes do Update da cena).</summary>
        public static void Update()
        {
            _kbPrev = _kbNow;
            _kbNow = Keyboard.GetState();
            _padPrev = _padNow;
            _padNow = GamePad.GetState(PlayerIndex.One);
        }

        /// <summary>Define/atualiza o mapeamento de uma ação.</summary>
        public static void Map(string action, Keys[] keys, Buttons[]? buttons = null)
            => _map[action] = new Binding(keys, buttons ?? System.Array.Empty<Buttons>());

        public static bool IsDown(string action) => IsDown(action, _kbNow, _padNow);
        public static bool JustPressed(string action) => IsDown(action, _kbNow, _padNow) && !IsDown(action, _kbPrev, _padPrev);
        public static bool JustReleased(string action) => !IsDown(action, _kbNow, _padNow) && IsDown(action, _kbPrev, _padPrev);

        /// <summary>Eixo horizontal em [-1,1] (MoveRight - MoveLeft), com thumbstick analógico.</summary>
        public static float Horizontal()
        {
            float v = (IsDown("MoveRight") ? 1f : 0f) - (IsDown("MoveLeft") ? 1f : 0f);
            float stick = _padNow.ThumbSticks.Left.X;
            if (System.Math.Abs(stick) > 0.2f)
                v = stick;
            return MathHelper.Clamp(v, -1f, 1f);
        }

        private static bool IsDown(string action, KeyboardState kb, GamePadState pad)
        {
            if (!_map.TryGetValue(action, out var b))
                return false;

            foreach (var key in b.Keys)
                if (kb.IsKeyDown(key))
                    return true;

            if (pad.IsConnected)
            {
                foreach (var btn in b.Buttons)
                    if (pad.IsButtonDown(btn))
                        return true;
            }
            return false;
        }
    }
}
