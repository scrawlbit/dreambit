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
        private static readonly System.Text.StringBuilder _typedPending = new();
        private static string _typedFrame = string.Empty;

        /// <summary>Texto digitado neste frame (para campos de texto). O host injeta com
        /// <see cref="PushText"/> a partir do evento de texto da janela.</summary>
        public static string TypedText => _typedFrame;

        /// <summary>Host chama para cada caractere digitado (evento TextInput da janela).</summary>
        public static void PushText(char c) => _typedPending.Append(c);

        /// <summary>Torna o texto acumulado disponível em <see cref="TypedText"/> para este frame
        /// e reinicia o acúmulo. Chamado pelo <see cref="Update"/>; exposto para hosts que não
        /// chamam Update (editor) ou testes.</summary>
        public static void PumpText()
        {
            _typedFrame = _typedPending.ToString();
            _typedPending.Clear();
        }

        /// <summary>True se a tecla foi pressionada neste frame (borda).</summary>
        public static bool KeyJustPressed(Keys key) => _kbNow.IsKeyDown(key) && !_kbPrev.IsKeyDown(key);
        private static MouseState _mouseNow, _mousePrev;
        private static bool _mouseOverride;
        private static Vector2 _mouseOverridePos;
        private static bool _mouseOverrideDown, _mouseOverrideDownPrev;
        private static int _wheelDelta;
        private static float _overrideWheel;

        /// <summary>Snapshot do frame (chame uma vez por frame antes do Update da cena).</summary>
        public static void Update()
        {
            _kbPrev = _kbNow;
            _kbNow = Keyboard.GetState();
            _padPrev = _padNow;
            _padNow = GamePad.GetState(PlayerIndex.One);

            // Rotaciona as ações forçadas: o que foi mantido no frame anterior vira "prev".
            (_forcedPrev, _forcedNow) = (_forcedNow, _forcedPrev);
            _forcedNow.Clear();

            // Double-buffer do texto digitado: o que foi acumulado desde o último frame vira o
            // texto deste frame (estável para os campos lerem), e o pendente reinicia.
            PumpText();
            if (!_mouseOverride)
            {
                _mousePrev = _mouseNow;
                _mouseNow = Mouse.GetState();
                _wheelDelta = (_mouseNow.ScrollWheelValue - _mousePrev.ScrollWheelValue) / 120; // detents
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

        /// <summary>Passos da roda do mouse neste frame (+ para cima, - para baixo). O host sem
        /// mouse do MonoGame pode injetar com <see cref="SetWheel"/>.</summary>
        public static int WheelDelta => _mouseOverride ? (int)_overrideWheel : _wheelDelta;

        /// <summary>Host (editor) injeta passos de roda para este frame.</summary>
        public static void SetWheel(float steps) => _overrideWheel = steps;

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

        // Ações forçadas por software (host sem teclado real: preview do editor, testes).
        // Rotacionadas como o teclado em Update() para preservar a detecção de borda.
        private static HashSet<string> _forcedNow = new();
        private static HashSet<string> _forcedPrev = new();

        /// <summary>Mantém uma ação "pressionada" neste frame por software (chame após
        /// <see cref="Update"/>, antes de ler o input). Útil para o preview do editor e testes.</summary>
        public static void HoldAction(string action)
        {
            if (!string.IsNullOrEmpty(action))
                _forcedNow.Add(action);
        }

        public static bool IsDown(string action) => IsDown(action, _kbNow, _padNow, _forcedNow);
        public static bool JustPressed(string action)
            => IsDown(action, _kbNow, _padNow, _forcedNow) && !IsDown(action, _kbPrev, _padPrev, _forcedPrev);
        public static bool JustReleased(string action)
            => !IsDown(action, _kbNow, _padNow, _forcedNow) && IsDown(action, _kbPrev, _padPrev, _forcedPrev);

        /// <summary>Eixo horizontal em [-1,1] (MoveRight - MoveLeft), com thumbstick analógico.</summary>
        public static float Horizontal()
        {
            float v = (IsDown("MoveRight") ? 1f : 0f) - (IsDown("MoveLeft") ? 1f : 0f);
            float stick = _padNow.ThumbSticks.Left.X;
            if (System.Math.Abs(stick) > 0.2f)
                v = stick;
            return MathHelper.Clamp(v, -1f, 1f);
        }

        private static bool IsDown(string action, KeyboardState kb, GamePadState pad, HashSet<string> forced)
        {
            if (forced.Contains(action))
                return true;

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
