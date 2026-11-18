using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DreamBit.Engine.Input
{
    /// <summary>Qual parte do teclado um jogador usa.</summary>
    public enum KeyboardScheme
    {
        /// <summary>Sem teclado (só controle).</summary>
        None,
        /// <summary>Teclado inteiro (setas e WASD), jogando sozinho.</summary>
        Full,
        /// <summary>Lado esquerdo no co-op de um teclado: WASD, Espaço, F/G/R/Q/E/Shift esquerdo.</summary>
        Left,
        /// <summary>Lado direito no co-op de um teclado: setas, numpad (0/1/2/3/Enter), Ctrl/Shift direitos.</summary>
        Right
    }

    /// <summary>
    /// Input de um jogador local: um esquema de teclado + um controle, com mapa de ações próprio
    /// (rebind por jogador). Lê os estados capturados em <see cref="Input.Update"/>.
    /// </summary>
    public sealed class PlayerInput : IInputSource
    {
        private sealed record Binding(Keys[] Keys, Buttons[] Buttons);

        private readonly Dictionary<string, Binding> _map = new(StringComparer.Ordinal);
        private readonly Dictionary<string, Keys[]> _schemeKeys = new(StringComparer.Ordinal);
        private KeyboardScheme _scheme;
        private float _rumbleTime;

        public PlayerInput(int index, KeyboardScheme scheme, int gamepad)
        {
            Index = index;
            Gamepad = gamepad;
            Scheme = scheme;
            foreach (var (action, buttons) in DefaultButtons)
                _map[action] = new Binding(Array.Empty<Keys>(), buttons);
        }

        /// <summary>Índice do jogador (0 = P1).</summary>
        public int Index { get; }

        /// <summary>Controle usado (0..3), ou -1 para nenhum.</summary>
        public int Gamepad { get; set; }

        /// <summary>Zona morta dos analógicos.</summary>
        public float DeadZone { get; set; } = 0.25f;

        /// <summary>Esquema de teclado; trocar recarrega as teclas padrão (rebinds de tecla são refeitos).</summary>
        public KeyboardScheme Scheme
        {
            get => _scheme;
            set
            {
                _scheme = value;
                _schemeKeys.Clear();
                foreach (var (action, keys) in DefaultKeys(value))
                    _schemeKeys[action] = keys;
            }
        }

        /// <summary>True se o controle deste jogador está conectado.</summary>
        public bool IsGamepadConnected => Gamepad >= 0 && Input.PadNow(Gamepad).IsConnected;

        /// <summary>Redefine as teclas/botões de uma ação para este jogador.</summary>
        public void Map(string action, Keys[]? keys, Buttons[]? buttons)
        {
            if (keys != null) _schemeKeys[action] = keys;
            var current = _map.TryGetValue(action, out var b) ? b.Buttons : Array.Empty<Buttons>();
            _map[action] = new Binding(Array.Empty<Keys>(), buttons ?? current);
        }

        /// <summary>Teclas atualmente ligadas a uma ação (para telas de configuração).</summary>
        public IReadOnlyList<Keys> KeysFor(string action) => _schemeKeys.TryGetValue(action, out var k) ? k : Array.Empty<Keys>();

        /// <summary>Botões atualmente ligados a uma ação.</summary>
        public IReadOnlyList<Buttons> ButtonsFor(string action) => _map.TryGetValue(action, out var b) ? b.Buttons : Array.Empty<Buttons>();

        public bool IsDown(string action) => Down(action, previous: false);
        public bool JustPressed(string action) => Down(action, false) && !Down(action, true);
        public bool JustReleased(string action) => !Down(action, false) && Down(action, true);

        public float Horizontal()
        {
            float v = (IsDown("MoveRight") ? 1f : 0f) - (IsDown("MoveLeft") ? 1f : 0f);
            if (Gamepad >= 0)
            {
                float stick = Input.PadNow(Gamepad).ThumbSticks.Left.X;
                if (Math.Abs(stick) > DeadZone) v = stick;
            }
            return MathHelper.Clamp(v, -1f, 1f);
        }

        public float Vertical()
        {
            float v = (IsDown("MoveDown") ? 1f : 0f) - (IsDown("MoveUp") ? 1f : 0f);
            if (Gamepad >= 0)
            {
                float stick = -Input.PadNow(Gamepad).ThumbSticks.Left.Y;
                if (Math.Abs(stick) > DeadZone) v = stick;
            }
            return MathHelper.Clamp(v, -1f, 1f);
        }

        /// <summary>Analógico direito (mira), em [-1,1] (Y para baixo positivo).</summary>
        public Vector2 Aim()
        {
            if (Gamepad < 0) return Vector2.Zero;
            var s = Input.PadNow(Gamepad).ThumbSticks.Right;
            var v = new Vector2(s.X, -s.Y);
            return v.Length() > DeadZone ? v : Vector2.Zero;
        }

        /// <summary>Vibra o controle por <paramref name="duration"/> s (0..1 em cada motor).</summary>
        public void Rumble(float left, float right, float duration)
        {
            if (Gamepad < 0) return;
            GamePad.SetVibration(Gamepad, MathHelper.Clamp(left, 0f, 1f), MathHelper.Clamp(right, 0f, 1f));
            _rumbleTime = Math.Max(_rumbleTime, duration);
        }

        internal void Tick(float dt)
        {
            if (_rumbleTime <= 0f) return;
            _rumbleTime -= dt;
            if (_rumbleTime <= 0f && Gamepad >= 0)
                GamePad.SetVibration(Gamepad, 0f, 0f);
        }

        private bool Down(string action, bool previous)
        {
            if (Index == 0 && Input.IsForced(action, previous))
                return true;

            var kb = previous ? Input.KeyboardPrev : Input.KeyboardNow;
            if (_schemeKeys.TryGetValue(action, out var keys))
                foreach (var key in keys)
                    if (kb.IsKeyDown(key))
                        return true;

            if (Gamepad >= 0 && _map.TryGetValue(action, out var b))
            {
                var pad = previous ? Input.PadPrev(Gamepad) : Input.PadNow(Gamepad);
                if (pad.IsConnected)
                    foreach (var btn in b.Buttons)
                        if (pad.IsButtonDown(btn))
                            return true;
            }
            return false;
        }

        // ---- mapas padrão ----

        private static readonly (string, Buttons[])[] DefaultButtons =
        {
            ("MoveLeft", new[] { Buttons.DPadLeft, Buttons.LeftThumbstickLeft }),
            ("MoveRight", new[] { Buttons.DPadRight, Buttons.LeftThumbstickRight }),
            ("MoveUp", new[] { Buttons.DPadUp, Buttons.LeftThumbstickUp }),
            ("MoveDown", new[] { Buttons.DPadDown, Buttons.LeftThumbstickDown }),
            ("Jump", new[] { Buttons.A }),
            ("Attack", new[] { Buttons.X }),
            ("Special", new[] { Buttons.Y }),
            ("Dash", new[] { Buttons.RightTrigger, Buttons.RightShoulder }),
            ("Action", new[] { Buttons.B }),
            ("Swap", new[] { Buttons.LeftShoulder }),
            ("Ability", new[] { Buttons.LeftTrigger }),
            ("Pause", new[] { Buttons.Start }),
            ("Map", new[] { Buttons.Back }),
            ("Confirm", new[] { Buttons.A }),
            ("Cancel", new[] { Buttons.B }),
        };

        private static IEnumerable<(string, Keys[])> DefaultKeys(KeyboardScheme scheme)
        {
            switch (scheme)
            {
                case KeyboardScheme.Full:
                    yield return ("MoveLeft", new[] { Keys.Left, Keys.A });
                    yield return ("MoveRight", new[] { Keys.Right, Keys.D });
                    yield return ("MoveUp", new[] { Keys.Up, Keys.W });
                    yield return ("MoveDown", new[] { Keys.Down, Keys.S });
                    yield return ("Jump", new[] { Keys.Space, Keys.Z });
                    yield return ("Attack", new[] { Keys.J, Keys.X });
                    yield return ("Special", new[] { Keys.K, Keys.C });
                    yield return ("Dash", new[] { Keys.LeftShift, Keys.L });
                    yield return ("Action", new[] { Keys.E, Keys.Enter });
                    yield return ("Swap", new[] { Keys.Tab, Keys.Q });
                    yield return ("Ability", new[] { Keys.F, Keys.V });
                    yield return ("Pause", new[] { Keys.Escape });
                    yield return ("Map", new[] { Keys.M });
                    yield return ("Confirm", new[] { Keys.Enter, Keys.Space });
                    yield return ("Cancel", new[] { Keys.Escape, Keys.Back });
                    break;
                case KeyboardScheme.Left:
                    yield return ("MoveLeft", new[] { Keys.A });
                    yield return ("MoveRight", new[] { Keys.D });
                    yield return ("MoveUp", new[] { Keys.W });
                    yield return ("MoveDown", new[] { Keys.S });
                    yield return ("Jump", new[] { Keys.Space });
                    yield return ("Attack", new[] { Keys.F });
                    yield return ("Special", new[] { Keys.G });
                    yield return ("Dash", new[] { Keys.LeftShift });
                    yield return ("Action", new[] { Keys.E });
                    yield return ("Swap", new[] { Keys.Q });
                    yield return ("Ability", new[] { Keys.R });
                    yield return ("Pause", new[] { Keys.Escape });
                    yield return ("Map", new[] { Keys.Tab });
                    yield return ("Confirm", new[] { Keys.Space, Keys.E });
                    yield return ("Cancel", new[] { Keys.Escape });
                    break;
                case KeyboardScheme.Right:
                    yield return ("MoveLeft", new[] { Keys.Left });
                    yield return ("MoveRight", new[] { Keys.Right });
                    yield return ("MoveUp", new[] { Keys.Up });
                    yield return ("MoveDown", new[] { Keys.Down });
                    yield return ("Jump", new[] { Keys.NumPad0, Keys.RightControl });
                    yield return ("Attack", new[] { Keys.NumPad1 });
                    yield return ("Special", new[] { Keys.NumPad2 });
                    yield return ("Dash", new[] { Keys.RightShift });
                    yield return ("Action", new[] { Keys.Enter });
                    yield return ("Swap", new[] { Keys.NumPad3 });
                    yield return ("Ability", new[] { Keys.NumPad4 });
                    yield return ("Pause", new[] { Keys.Back });
                    yield return ("Map", new[] { Keys.NumPad5 });
                    yield return ("Confirm", new[] { Keys.Enter });
                    yield return ("Cancel", new[] { Keys.Back });
                    break;
            }
        }
    }

    /// <summary>Os jogadores locais (P1..P4).</summary>
    public sealed class PlayerInputSet
    {
        private readonly PlayerInput[] _players = new PlayerInput[Input.MaxPlayers];

        internal PlayerInputSet()
        {
            _players[0] = new PlayerInput(0, KeyboardScheme.Full, 0);
            for (int i = 1; i < _players.Length; i++)
                _players[i] = new PlayerInput(i, KeyboardScheme.None, i);
        }

        public PlayerInput this[int index] => _players[index];

        public int Count => _players.Length;

        /// <summary>Um jogador: teclado inteiro + controle 1.</summary>
        public void ConfigureSinglePlayer()
        {
            _players[0].Scheme = KeyboardScheme.Full; _players[0].Gamepad = 0;
            for (int i = 1; i < _players.Length; i++) { _players[i].Scheme = KeyboardScheme.None; _players[i].Gamepad = i; }
        }

        /// <summary>Co-op local de dois. Com <paramref name="sharedKeyboard"/>, P1 usa o lado esquerdo
        /// do teclado e P2 o direito; os controles 1 e 2 funcionam sempre.</summary>
        public void ConfigureCoop(bool sharedKeyboard)
        {
            _players[0].Scheme = sharedKeyboard ? KeyboardScheme.Left : KeyboardScheme.Full;
            _players[0].Gamepad = 0;
            _players[1].Scheme = sharedKeyboard ? KeyboardScheme.Right : KeyboardScheme.None;
            _players[1].Gamepad = 1;
        }

        internal void Tick(float dt)
        {
            foreach (var p in _players)
                p.Tick(dt);
        }
    }

    /// <summary>
    /// Fonte de input controlada por código (IA, cutscenes, replays): o dono define quais
    /// ações estão pressionadas e os eixos. A borda (JustPressed/JustReleased) vale para o
    /// primeiro frame (<see cref="Input.Frame"/>) em que alguém lê a ação depois da mudança —
    /// então funciona chamando <see cref="Set"/> antes ou depois do <c>Input.Update()</c>.
    /// </summary>
    public sealed class VirtualInput : IInputSource
    {
        private const long Pending = -1;
        private readonly HashSet<string> _down = new(StringComparer.Ordinal);
        private readonly Dictionary<string, long> _pressEdge = new(StringComparer.Ordinal);
        private readonly Dictionary<string, long> _releaseEdge = new(StringComparer.Ordinal);
        private float _h, _v;

        /// <summary>Mantém (ou solta) uma ação até mudar de novo.</summary>
        public void Set(string action, bool down)
        {
            if (down)
            {
                if (_down.Add(action)) { _pressEdge[action] = Pending; _releaseEdge.Remove(action); }
            }
            else if (_down.Remove(action))
            {
                _releaseEdge[action] = Pending;
                _pressEdge.Remove(action);
            }
        }

        /// <summary>Aperta e solta no próximo frame lido (um toque).</summary>
        public void Tap(string action)
        {
            Set(action, true);
            _tapPending.Add(action);
        }

        private readonly HashSet<string> _tapPending = new(StringComparer.Ordinal);

        /// <summary>Solta todas as ações e zera os eixos.</summary>
        public void ReleaseAll()
        {
            foreach (var a in new List<string>(_down))
                Set(a, false);
            _h = _v = 0f;
        }

        /// <summary>Define os eixos ([-1,1]).</summary>
        public void SetAxes(float horizontal, float vertical = 0f)
        {
            _h = MathHelper.Clamp(horizontal, -1f, 1f);
            _v = MathHelper.Clamp(vertical, -1f, 1f);
        }

        public bool IsDown(string action)
        {
            bool down = _down.Contains(action);
            ReleaseTapIfConsumed(action);
            return down;
        }

        public bool JustPressed(string action) => Edge(_pressEdge, action);
        public bool JustReleased(string action) => Edge(_releaseEdge, action);
        public float Horizontal() => _h != 0f ? _h : (IsDown("MoveRight") ? 1f : 0f) - (IsDown("MoveLeft") ? 1f : 0f);
        public float Vertical() => _v != 0f ? _v : (IsDown("MoveDown") ? 1f : 0f) - (IsDown("MoveUp") ? 1f : 0f);

        private bool Edge(Dictionary<string, long> edges, string action)
        {
            if (!edges.TryGetValue(action, out var frame))
                return false;
            if (frame == Pending)
            {
                edges[action] = Input.Frame; // observado agora: vale por este frame inteiro
                return true;
            }
            if (frame == Input.Frame)
                return true;
            edges.Remove(action);
            ReleaseTapIfConsumed(action);
            return false;
        }

        // Um Tap solta a ação depois que o frame em que ela foi vista passou.
        private void ReleaseTapIfConsumed(string action)
        {
            if (_tapPending.Contains(action) && _pressEdge.TryGetValue(action, out var f) && f != Pending && f != Input.Frame)
            {
                _tapPending.Remove(action);
                Set(action, false);
            }
        }
    }
}
