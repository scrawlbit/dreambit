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
        private static MouseState _mouseNow, _mousePrev;
        private static bool _mouseOverride;
        private static Vector2 _mouseOverridePos;
        private static bool _mouseOverrideDown, _mouseOverrideDownPrev;

        /// <summary>Snapshot do frame (chame uma vez por frame antes do Update da cena).</summary>
        public static void Update()
        {
            _kbPrev = _kbNow;
            _kbNow = Keyboard.GetState();
            _padPrev = _padNow;
            _padNow = GamePad.GetState(PlayerIndex.One);
            if (!_mouseOverride)
            {
                _mousePrev = _mouseNow;
                _mouseNow = Mouse.GetState();
            }
            // Com override (editor), o estado do ponteiro é avançado em SetPointer, não aqui.
        }

        /// <summary>Ponteiro (mouse/toque) em coordenadas de tela. O host pode sobrescrever
        /// via <see cref="SetPointer"/> (ex.: editor Avalonia, que não tem mouse do MonoGame).</summary>
        public static Vector2 PointerPosition => _mouseOverride
            ? _mouseOverridePos
            : new Vector2(_mouseNow.X, _mouseNow.Y);

        /// <summary>True enquanto o botão principal (clique/toque) está pressionado.</summary>
        public static bool PointerDown => _mouseOverride
            ? _mouseOverrideDown
            : _mouseNow.LeftButton == ButtonState.Pressed;

        /// <summary>True no frame em que o clique/toque começou.</summary>
        public static bool PointerPressed => _mouseOverride
            ? _mouseOverrideDown && !_mouseOverrideDownPrev
            : _mouseNow.LeftButton == ButtonState.Pressed && _mousePrev.LeftButton != ButtonState.Pressed;

        /// <summary>True no frame em que o clique/toque terminou.</summary>
        public static bool PointerReleased => _mouseOverride
            ? !_mouseOverrideDown && _mouseOverrideDownPrev
            : _mouseNow.LeftButton != ButtonState.Pressed && _mousePrev.LeftButton == ButtonState.Pressed;

        /// <summary>Host sem mouse do MonoGame (editor) injeta o ponteiro aqui, todo frame.</summary>
        public static void SetPointer(Vector2 position, bool down)
        {
            _mouseOverride = true;
            _mouseOverrideDownPrev = _mouseOverrideDown;
            _mouseOverridePos = position;
            _mouseOverrideDown = down;
        }

        /// <summary>Volta a usar o mouse do MonoGame (desfaz <see cref="SetPointer"/>).</summary>
        public static void ClearPointerOverride() => _mouseOverride = false;

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
