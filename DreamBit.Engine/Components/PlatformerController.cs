using System;
using System.Collections.Generic;
using System.Linq;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Physics;
using Microsoft.Xna.Framework;
using GameInput = DreamBit.Engine.Input.Input;

namespace DreamBit.Engine.Components
{
    /// <summary>Estado de movimento do <see cref="PlatformerController"/> (para animação e jogo).</summary>
    public enum PlatformerState
    {
        Idle, Run, Jump, Fall, WallSlide, LedgeHang, LedgeClimb, Dash, Slide, Crouch, Glide, GroundPound
    }

    /// <summary>Habilidades de movimento liberáveis (metroidvania: começam desligadas e são
    /// destravadas pelo jogo).</summary>
    [Flags]
    public enum MovementAbilities
    {
        None = 0,
        DoubleJump = 1 << 0,
        WallJump = 1 << 1,
        LedgeGrab = 1 << 2,
        Dash = 1 << 3,
        Slide = 1 << 4,
        Glide = 1 << 5,
        GroundPound = 1 << 6,
        Push = 1 << 7,
        DropThrough = 1 << 8,
        Crouch = 1 << 9,
        /// <summary>O dash quebra <see cref="Breakable"/> com BreakBy "Dash".</summary>
        BreakWalls = 1 << 10,
        /// <summary>A pancada no chão quebra <see cref="Breakable"/> com BreakBy "GroundPound".</summary>
        BreakFloors = 1 << 11,
        All = ~0
    }

    /// <summary>
    /// Controlador de plataforma: gravidade, colisão sólida (AABB) e ledges one-way, com
    /// aceleração, coyote time, buffer e altura variável de pulo, e habilidades liberáveis
    /// (<see cref="Abilities"/>): pulo duplo, planar, deslizar/pular na parede, agarrar bordas,
    /// dash, agachar/deslizar no chão, pancada no chão, empurrar objetos e atravessar plataformas.
    /// Lê comandos de uma <see cref="Input.IInputSource"/> (jogador, IA ou global).
    /// Ações usadas: MoveLeft/Right/Up/Down, Jump, Dash, Attack (pancada: no ar, baixo + ataque).
    /// </summary>
    public sealed class PlatformerController : SceneComponent, Input.IInputControlled
    {
        // ---- configuração básica (compatível com cenas antigas) ----
        private float _gravity = 1400f;
        private float _halfHeight = 24f;
        private float _halfWidth = 24f;
        private float _horizontalSpeed;
        private float _moveSpeed = 220f;
        private float _jumpSpeed = 620f;
        private bool _useKeyboard = true;
        private int _playerIndex = -1;

        // ---- sensação de controle ----
        private float _acceleration;          // 0 = instantâneo (comportamento antigo)
        private float _deceleration;
        private float _airControl = 1f;
        private float _maxFallSpeed = 1600f;
        private float _coyoteTime = 0.1f;
        private float _jumpBufferTime = 0.12f;
        private float _jumpCutMultiplier = 0.5f;

        // ---- habilidades ----
        private MovementAbilities _abilities = MovementAbilities.DropThrough | MovementAbilities.Crouch;
        private int _airJumps = 1;
        private float _airJumpSpeed = 560f;
        private float _glideFallSpeed = 120f;
        private float _wallSlideSpeed = 160f;
        private float _wallJumpSpeedX = 360f;
        private float _wallJumpSpeedY = 600f;
        private float _wallJumpLockTime = 0.15f;
        private float _ledgeGrabReach = 18f;
        private float _ledgeClimbTime = 0.25f;
        private float _dashSpeed = 720f;
        private float _dashDuration = 0.18f;
        private float _dashCooldown = 0.35f;
        private int _airDashes = 1;
        private float _crouchHalfHeight = 14f;
        private float _crouchSpeedFactor = 0.45f;
        private float _slideSpeed = 520f;
        private float _slideDuration = 0.45f;
        private float _groundPoundSpeed = 1400f;
        private float _pushSpeedFactor = 0.5f;

        // ---- estado ----
        private Vector2 _velocity;
        private bool _grounded;
        private bool _groundIsOneWay;
        private float _currentVx;
        private float _coyote;
        private float _jumpBuffer;
        private bool _jumping;
        private int _airJumpsLeft;
        private int _airDashesLeft;
        private float _wallLock;
        private int _wallDir;
        private float _dashTime;
        private float _dashCooldownLeft;
        private float _slideTime;
        private float _dropThrough;
        private float _climbTime;
        private Vector2 _climbTarget;
        private bool _crouching;
        private GameObject? _ground;
        private Vector2 _groundLastPos;
        private PlatformerState _state;

        public override string DisplayName => "Platformer Controller";

        // ================== propriedades ==================

        /// <summary>Jogador que controla (0 = P1, 1 = P2…); -1 = mapa global (teclado + controle 1).</summary>
        public int PlayerIndex { get => _playerIndex; set => Set(ref _playerIndex, Math.Clamp(value, -1, GameInput.MaxPlayers - 1)); }

        /// <summary>Fonte de input em runtime (IA, cutscene); tem prioridade sobre <see cref="PlayerIndex"/>. Não é salva.</summary>
        public Input.IInputSource? InputOverride { get; set; }

        /// <summary>Fonte efetiva: <see cref="InputOverride"/> ou o jogador de <see cref="PlayerIndex"/>.</summary>
        public Input.IInputSource InputSource => InputOverride ?? GameInput.ForPlayer(_playerIndex);

        /// <summary>True quando está no chão (para máquina de estados de animação).</summary>
        public bool Grounded => _grounded;

        /// <summary>Velocidade horizontal atual (px/s) neste frame.</summary>
        public float CurrentVelocityX => _currentVx;

        /// <summary>Velocidade atual (px/s). Pode ser alterada (empurrões, molas).</summary>
        public Vector2 Velocity { get => _velocity; set => _velocity = value; }

        /// <summary>True se está se movendo na horizontal.</summary>
        public bool IsMovingHorizontally => Math.Abs(_currentVx) > 1f;

        /// <summary>Direção para onde olha (+1 direita, -1 esquerda).</summary>
        public int Facing { get; private set; } = 1;

        /// <summary>Estado de movimento atual.</summary>
        public PlatformerState State => _state;

        /// <summary>True durante o dash (o jogo pode dar invulnerabilidade).</summary>
        public bool IsDashing => _dashTime > 0f;

        /// <summary>Meia-altura em uso agora (menor ao agachar/deslizar).</summary>
        public float CurrentHalfHeight => _crouching ? Math.Min(_crouchHalfHeight, _halfHeight) : _halfHeight;

        /// <summary>Trava o input (cutscene, diálogo) mantendo a física.</summary>
        public bool InputLocked { get; set; }

        /// <summary>Aceleração da gravidade (px/s²).</summary>
        public float Gravity { get => _gravity; set => Set(ref _gravity, value); }

        /// <summary>Distância do centro até os "pés" do objeto.</summary>
        public float HalfHeight { get => _halfHeight; set => Set(ref _halfHeight, Math.Max(0f, value)); }

        /// <summary>Meia-largura do objeto (para colisão sólida por AABB).</summary>
        public float HalfWidth { get => _halfWidth; set => Set(ref _halfWidth, Math.Max(0f, value)); }

        /// <summary>Velocidade horizontal constante (px/s) quando não é por input.</summary>
        public float HorizontalSpeed { get => _horizontalSpeed; set => Set(ref _horizontalSpeed, value); }

        /// <summary>Controlar por input (personagem jogável ou IA).</summary>
        public bool UseKeyboard { get => _useKeyboard; set => Set(ref _useKeyboard, value); }

        /// <summary>Velocidade ao andar (px/s).</summary>
        public float MoveSpeed { get => _moveSpeed; set => Set(ref _moveSpeed, value); }

        /// <summary>Impulso de pulo (px/s).</summary>
        public float JumpSpeed { get => _jumpSpeed; set => Set(ref _jumpSpeed, value); }

        /// <summary>Aceleração (px/s²); 0 = troca de velocidade instantânea.</summary>
        public float Acceleration { get => _acceleration; set => Set(ref _acceleration, Math.Max(0f, value)); }

        /// <summary>Desaceleração ao soltar (px/s²); 0 = usa a aceleração.</summary>
        public float Deceleration { get => _deceleration; set => Set(ref _deceleration, Math.Max(0f, value)); }

        /// <summary>Fração da aceleração aplicada no ar (0..1).</summary>
        public float AirControl { get => _airControl; set => Set(ref _airControl, Math.Clamp(value, 0f, 1f)); }

        public float MaxFallSpeed { get => _maxFallSpeed; set => Set(ref _maxFallSpeed, Math.Max(0f, value)); }

        /// <summary>Tolerância (s) para pular logo depois de sair da borda.</summary>
        public float CoyoteTime { get => _coyoteTime; set => Set(ref _coyoteTime, Math.Max(0f, value)); }

        /// <summary>Tolerância (s) para um pulo apertado pouco antes de tocar o chão.</summary>
        public float JumpBufferTime { get => _jumpBufferTime; set => Set(ref _jumpBufferTime, Math.Max(0f, value)); }

        /// <summary>Multiplicador da subida ao soltar o pulo (altura variável; 1 = desligado).</summary>
        public float JumpCutMultiplier { get => _jumpCutMultiplier; set => Set(ref _jumpCutMultiplier, Math.Clamp(value, 0f, 1f)); }

        /// <summary>Habilidades liberadas.</summary>
        public MovementAbilities Abilities { get => _abilities; set => Set(ref _abilities, value); }

        public int AirJumps { get => _airJumps; set => Set(ref _airJumps, Math.Max(0, value)); }
        public float AirJumpSpeed { get => _airJumpSpeed; set => Set(ref _airJumpSpeed, Math.Max(0f, value)); }
        public float GlideFallSpeed { get => _glideFallSpeed; set => Set(ref _glideFallSpeed, Math.Max(0f, value)); }
        public float WallSlideSpeed { get => _wallSlideSpeed; set => Set(ref _wallSlideSpeed, Math.Max(0f, value)); }
        public float WallJumpSpeedX { get => _wallJumpSpeedX; set => Set(ref _wallJumpSpeedX, Math.Max(0f, value)); }
        public float WallJumpSpeedY { get => _wallJumpSpeedY; set => Set(ref _wallJumpSpeedY, Math.Max(0f, value)); }
        public float WallJumpLockTime { get => _wallJumpLockTime; set => Set(ref _wallJumpLockTime, Math.Max(0f, value)); }
        /// <summary>Altura (px abaixo do topo da cabeça) em que uma quina pode ser agarrada.</summary>
        public float LedgeGrabReach { get => _ledgeGrabReach; set => Set(ref _ledgeGrabReach, Math.Max(1f, value)); }
        public float LedgeClimbTime { get => _ledgeClimbTime; set => Set(ref _ledgeClimbTime, Math.Max(0f, value)); }
        public float DashSpeed { get => _dashSpeed; set => Set(ref _dashSpeed, Math.Max(0f, value)); }
        public float DashDuration { get => _dashDuration; set => Set(ref _dashDuration, Math.Max(0.01f, value)); }
        public float DashCooldown { get => _dashCooldown; set => Set(ref _dashCooldown, Math.Max(0f, value)); }
        public int AirDashes { get => _airDashes; set => Set(ref _airDashes, Math.Max(0, value)); }
        public float CrouchHalfHeight { get => _crouchHalfHeight; set => Set(ref _crouchHalfHeight, Math.Max(1f, value)); }
        public float CrouchSpeedFactor { get => _crouchSpeedFactor; set => Set(ref _crouchSpeedFactor, Math.Clamp(value, 0f, 1f)); }
        public float SlideSpeed { get => _slideSpeed; set => Set(ref _slideSpeed, Math.Max(0f, value)); }
        public float SlideDuration { get => _slideDuration; set => Set(ref _slideDuration, Math.Max(0.01f, value)); }
        public float GroundPoundSpeed { get => _groundPoundSpeed; set => Set(ref _groundPoundSpeed, Math.Max(0f, value)); }
        /// <summary>Fração da velocidade ao empurrar um <see cref="Pushable"/>.</summary>
        public float PushSpeedFactor { get => _pushSpeedFactor; set => Set(ref _pushSpeedFactor, Math.Clamp(value, 0f, 1f)); }

        /// <summary>Envia o estado para uma <see cref="AnimationStateMachine"/> do mesmo objeto
        /// (bools grounded, moving, falling, wallSlide, ledgeHang, ledgeClimb, dashing, sliding,
        /// crouching, gliding, groundPound e gatilhos jump, airJump, wallJump, land, dash,
        /// groundPound, groundPoundLand, ledgeGrab, ledgeClimb).</summary>
        public bool FeedAnimator { get; set; } = true;

        /// <summary>Envia os eventos também como mensagens da cena ("platformer:jump" etc.).</summary>
        public bool SendMessages { get; set; }

        public event Action<PlatformerController>? Jumped;
        public event Action<PlatformerController>? AirJumped;
        public event Action<PlatformerController>? WallJumped;
        public event Action<PlatformerController>? Landed;
        public event Action<PlatformerController>? DashStarted;
        public event Action<PlatformerController>? GroundPoundLanded;
        public event Action<PlatformerController>? LedgeGrabbed;

        public bool Has(MovementAbilities ability) => (_abilities & ability) == ability;

        /// <summary>Libera (ou bloqueia) uma habilidade em runtime.</summary>
        public void Unlock(MovementAbilities ability, bool enabled = true)
            => Abilities = enabled ? _abilities | ability : _abilities & ~ability;

        public void Reset()
        {
            _velocity = Vector2.Zero;
            _grounded = false;
            _dashTime = _slideTime = _climbTime = 0f;
            _crouching = false;
            _ground = null;
            _state = PlatformerState.Idle;
            _airJumpsLeft = _airJumps;
            _airDashesLeft = _airDashes;
        }

        /// <summary>Teleporta (zera velocidade e estados especiais).</summary>
        public void Teleport(Vector2 position)
        {
            Reset();
            Owner.Transform.Position = position;
        }

        /// <summary>Soma um impulso (knockback, mola).</summary>
        public void AddImpulse(Vector2 impulse)
        {
            _velocity += impulse;
            if (impulse.Y < 0f) { _grounded = false; _coyote = 0f; }
        }

        protected internal override void OnPlayStarted() => Reset();

        // ================== simulação ==================

        protected internal override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (dt <= 0f)
                return;

            var scene = Owner.Scene;
            var solids = CollectSolids(scene, out var owners);
            var input = InputSource;
            bool control = _useKeyboard && !InputLocked;

            float h = control ? input.Horizontal() : 0f;
            bool down = control && input.Vertical() > 0.5f;
            bool up = control && input.Vertical() < -0.5f;
            bool jumpPressed = control && input.JustPressed("Jump");
            bool jumpHeld = control && input.IsDown("Jump");
            bool jumpReleased = control && input.JustReleased("Jump");
            bool dashPressed = control && input.JustPressed("Dash");
            bool attackPressed = control && input.JustPressed("Attack");

            TickTimers(dt);
            CarryWithGround();

            var pos = Owner.Transform.Position;

            // ---- subindo a borda (animação curta, depois vai para cima) ----
            if (_state == PlatformerState.LedgeClimb)
            {
                _climbTime -= dt;
                if (_climbTime <= 0f)
                {
                    Owner.Transform.Position = _climbTarget;
                    _state = PlatformerState.Idle;
                    _grounded = true;
                }
                _currentVx = 0f;
                FeedAnim();
                return;
            }

            // ---- pendurado na borda ----
            if (_state == PlatformerState.LedgeHang)
            {
                _velocity = Vector2.Zero;
                _currentVx = 0f;
                if (up || (jumpPressed && Math.Sign(h) != -Facing))
                {
                    _state = PlatformerState.LedgeClimb;
                    _climbTime = _ledgeClimbTime;
                    Fire(null, "ledgeClimb");
                }
                else if (down)
                {
                    _state = PlatformerState.Fall;
                    _dropThrough = 0.15f;
                    _ledgeRegrab = 0.3f;
                }
                else if (jumpPressed)
                {
                    _state = PlatformerState.Jump;
                    _velocity = new Vector2(-Facing * _wallJumpSpeedX, -_wallJumpSpeedY);
                    Facing = -Facing;
                    _wallLock = _wallJumpLockTime;
                    _jumping = true;
                    _ledgeRegrab = 0.3f;
                    Fire(WallJumped, "wallJump");
                }
                if (_state == PlatformerState.LedgeHang || _state == PlatformerState.LedgeClimb)
                {
                    FeedAnim();
                    return;
                }
            }

            // ---- buffer de pulo ----
            if (jumpPressed) _jumpBuffer = _jumpBufferTime;

            // ---- dash ----
            if (dashPressed && Has(MovementAbilities.Dash) && _dashTime <= 0f && _dashCooldownLeft <= 0f &&
                (_grounded || _airDashesLeft > 0) && !(down && _grounded && Has(MovementAbilities.Slide)))
            {
                if (!_grounded) _airDashesLeft--;
                if (Math.Abs(h) > 0.2f) Facing = Math.Sign(h);
                _dashTime = _dashDuration;
                _dashCooldownLeft = _dashCooldown + _dashDuration;
                _slideTime = 0f;
                if (_state == PlatformerState.GroundPound) _state = PlatformerState.Fall;
                Fire(DashStarted, "dash");
            }

            // ---- pancada no chão ----
            if (!_grounded && down && attackPressed && Has(MovementAbilities.GroundPound) && _state != PlatformerState.GroundPound)
            {
                _state = PlatformerState.GroundPound;
                _dashTime = 0f;
                Fire(null, "groundPound");
            }

            // ---- atravessar plataforma one-way (baixo + pulo, sem pular) ----
            if (_grounded && down && jumpPressed && _groundIsOneWay && Has(MovementAbilities.DropThrough))
            {
                _dropThrough = 0.25f;
                _jumpBuffer = 0f;
                _grounded = false;
                _groundIsOneWay = false;
            }

            // ---- deslizar no chão (baixo + dash, ou começar a agachar correndo) ----
            if (_grounded && down && Has(MovementAbilities.Slide) && _slideTime <= 0f && _dashTime <= 0f &&
                (dashPressed || (Math.Abs(_velocity.X) > _moveSpeed * 0.8f && input.JustPressed("MoveDown"))))
            {
                _slideTime = _slideDuration;
            }

            // ---- agachar (mantém agachado se não houver espaço para levantar) ----
            bool wantLow = (down && _grounded && Has(MovementAbilities.Crouch)) || _slideTime > 0f;
            SetCrouch(wantLow || (_crouching && !CanStand(solids, pos)), ref pos);

            // ---- velocidade horizontal ----
            float targetVx;
            bool instant = false;
            if (!_useKeyboard)
            {
                targetVx = _horizontalSpeed;
                instant = true;
            }
            else if (_dashTime > 0f) { targetVx = Facing * _dashSpeed; instant = true; }
            else if (_slideTime > 0f) { targetVx = Facing * _slideSpeed * Math.Max(0.35f, _slideTime / _slideDuration); instant = true; }
            else if (_state == PlatformerState.GroundPound) { targetVx = 0f; instant = true; }
            else if (_wallLock > 0f) targetVx = _velocity.X;
            else targetVx = h * _moveSpeed * (_crouching ? _crouchSpeedFactor : 1f);

            if (Math.Abs(h) > 0.2f && _dashTime <= 0f && _slideTime <= 0f && _wallLock <= 0f)
                Facing = Math.Sign(h);

            _velocity.X = instant ? targetVx : Approach(_velocity.X, targetVx, dt);

            // ---- pulo (chão/coyote, parede, ar) ----
            bool onWall = _wallDir != 0 && !_grounded;
            if (_jumpBuffer > 0f && _state != PlatformerState.GroundPound && (!_crouching || _slideTime > 0f) && _dropThrough <= 0f)
            {
                if (_grounded || _coyote > 0f)
                {
                    if (_crouching && CanStand(solids, pos)) SetCrouch(false, ref pos);
                    if (!_crouching)
                    {
                        _velocity.Y = -_jumpSpeed;
                        _jumping = true;
                        _grounded = false;
                        _coyote = 0f;
                        _jumpBuffer = 0f;
                        _slideTime = 0f;
                        Fire(Jumped, "jump");
                    }
                }
                else if (onWall && Has(MovementAbilities.WallJump))
                {
                    _velocity = new Vector2(-_wallDir * _wallJumpSpeedX, -_wallJumpSpeedY);
                    Facing = -_wallDir;
                    _wallLock = _wallJumpLockTime;
                    _jumping = true;
                    _jumpBuffer = 0f;
                    _airJumpsLeft = _airJumps;
                    Fire(WallJumped, "wallJump");
                }
                else if (jumpPressed && Has(MovementAbilities.DoubleJump) && _airJumpsLeft > 0)
                {
                    _airJumpsLeft--;
                    _velocity.Y = -_airJumpSpeed;
                    _jumping = true;
                    _jumpBuffer = 0f;
                    if (_state == PlatformerState.GroundPound) _state = PlatformerState.Jump;
                    Fire(AirJumped, "airJump");
                }
            }

            // Altura variável: soltar o pulo corta a subida.
            if (jumpReleased && _jumping && _velocity.Y < 0f)
            {
                _velocity.Y *= _jumpCutMultiplier;
                _jumping = false;
            }

            // ---- gravidade e quedas especiais ----
            if (_dashTime > 0f)
                _velocity.Y = 0f;
            else if (_state == PlatformerState.GroundPound)
                _velocity.Y = _groundPoundSpeed;
            else
            {
                _velocity.Y += _gravity * dt;
                bool pushingWall = onWall && Math.Sign(h) == _wallDir;
                if (_velocity.Y > 0f && pushingWall && Has(MovementAbilities.WallJump))
                    _velocity.Y = Math.Min(_velocity.Y, _wallSlideSpeed);
                else if (_velocity.Y > 0f && jumpHeld && !jumpPressed && Has(MovementAbilities.Glide))
                    _velocity.Y = Math.Min(_velocity.Y, _glideFallSpeed);
                _velocity.Y = Math.Min(_velocity.Y, _maxFallSpeed);
            }

            // ---- movimento horizontal + colisão (quebrar parede, empurrar) ----
            float halfH = CurrentHalfHeight;
            float x = pos.X + _velocity.X * dt;
            int hitIndex = FindOverlap(solids, x, pos.Y, halfH);
            if (hitIndex >= 0 && owners[hitIndex] is { } hitOwner)
            {
                if (_dashTime > 0f && Has(MovementAbilities.BreakWalls) &&
                    Breakable.TryBreak(hitOwner, Breakable.ByDash, Owner))
                {
                    solids.RemoveAt(hitIndex);
                    owners.RemoveAt(hitIndex);
                }
                else if (_grounded && _dashTime <= 0f && _slideTime <= 0f && Has(MovementAbilities.Push) &&
                         Get<Pushable>(hitOwner) is { } pushable && Math.Abs(_velocity.X) > 1f)
                {
                    float wanted = _velocity.X * _pushSpeedFactor * dt;
                    float moved = pushable.TryMove(wanted);
                    if (moved != 0f)
                    {
                        solids = CollectSolids(scene, out owners);
                        x = pos.X + moved;
                    }
                }
            }
            float resolvedX = SolidPhysics.ResolveX(solids, pos.X, x, pos.Y, _halfWidth, halfH);
            if (Math.Abs(resolvedX - x) > 0.001f)
            {
                _velocity.X = 0f;
                _slideTime = 0f;
                _dashTime = 0f;
            }
            x = resolvedX;

            // ---- movimento vertical + colisão ----
            bool wasGrounded = _grounded;
            float newY = pos.Y + _velocity.Y * dt;
            _grounded = false;
            _groundIsOneWay = false;
            _ground = null;
            var (resolvedY, grounded, ceiling) = SolidPhysics.ResolveY(solids, x, pos.Y, newY, _halfWidth, halfH);

            // Pancada no chão quebra o piso antes de pousar.
            if (grounded && _state == PlatformerState.GroundPound && Has(MovementAbilities.BreakFloors))
            {
                int below = FindBelow(solids, x, resolvedY, halfH);
                if (below >= 0 && owners[below] is { } floor && Breakable.TryBreak(floor, Breakable.ByGroundPound, Owner))
                {
                    solids.RemoveAt(below);
                    owners.RemoveAt(below);
                    (resolvedY, grounded, ceiling) = SolidPhysics.ResolveY(solids, x, pos.Y, newY, _halfWidth, halfH);
                    Fire(GroundPoundLanded, "groundPoundLand");
                }
            }

            newY = resolvedY;
            if (grounded)
            {
                _velocity.Y = 0f;
                _grounded = true;
                int below = FindBelow(solids, x, newY, halfH);
                if (below >= 0) SetGround(owners[below]);
            }
            else if (ceiling)
            {
                _velocity.Y = 0f;
                _jumping = false;
            }

            // Ledges (one-way): só pousa caindo e se não estiver atravessando.
            if (scene != null && _velocity.Y >= 0f && !_grounded && _dropThrough <= 0f)
            {
                float surface = LedgePhysics.FindLanding(scene.Ledges, x, pos.Y + halfH, newY + halfH);
                if (!float.IsNaN(surface))
                {
                    newY = surface - halfH;
                    _velocity.Y = 0f;
                    _grounded = true;
                    _groundIsOneWay = true;
                }
            }

            // ---- parede ao lado (wall slide/jump, agarrar borda) ----
            _wallDir = 0;
            if (!_grounded)
            {
                if (Touching(solids, x + 1f, newY, halfH)) _wallDir = 1;
                else if (Touching(solids, x - 1f, newY, halfH)) _wallDir = -1;
            }

            // ---- agarrar borda ----
            _ledgeRegrab = Math.Max(0f, _ledgeRegrab - dt);
            if (!_grounded && _velocity.Y >= 0f && Has(MovementAbilities.LedgeGrab) && _dashTime <= 0f && _ledgeRegrab <= 0f &&
                _state != PlatformerState.GroundPound && !_crouching &&
                TryGrabLedge(solids, x, newY, halfH, h, out var hang, out var top))
            {
                _velocity = Vector2.Zero;
                _state = PlatformerState.LedgeHang;
                _climbTarget = top;
                Owner.Transform.Position = hang;
                _currentVx = 0f;
                Fire(LedgeGrabbed, "ledgeGrab");
                FeedAnim();
                return;
            }

            // ---- pouso ----
            if (_grounded)
            {
                _coyote = _coyoteTime;
                _airJumpsLeft = _airJumps;
                _airDashesLeft = _airDashes;
                _jumping = false;
                if (!wasGrounded)
                {
                    if (_state == PlatformerState.GroundPound)
                        Fire(GroundPoundLanded, "groundPoundLand");
                    Fire(Landed, "land");
                }
            }

            Owner.Transform.Position = new Vector2(x, newY);
            _currentVx = _velocity.X;
            _state = ComputeState(h, jumpHeld);
            FeedAnim();
        }

        private float _ledgeRegrab;

        // ================== auxiliares ==================

        private void TickTimers(float dt)
        {
            _coyote = Math.Max(0f, _coyote - dt);
            _jumpBuffer = Math.Max(0f, _jumpBuffer - dt);
            _wallLock = Math.Max(0f, _wallLock - dt);
            _dashTime = Math.Max(0f, _dashTime - dt);
            _dashCooldownLeft = Math.Max(0f, _dashCooldownLeft - dt);
            _slideTime = Math.Max(0f, _slideTime - dt);
            _dropThrough = Math.Max(0f, _dropThrough - dt);
        }

        private float Approach(float current, float target, float dt)
        {
            if (_acceleration <= 0f)
                return target;
            bool speedingUp = Math.Abs(target) > Math.Abs(current) && (Math.Sign(target) == Math.Sign(current) || current == 0f);
            float rate = speedingUp || _deceleration <= 0f ? _acceleration : _deceleration;
            if (!_grounded) rate *= _airControl;
            if (rate <= 0f) return current;
            float delta = rate * dt;
            return current < target ? Math.Min(current + delta, target) : Math.Max(current - delta, target);
        }

        private PlatformerState ComputeState(float h, bool jumpHeld)
        {
            if (_state == PlatformerState.GroundPound && !_grounded) return PlatformerState.GroundPound;
            if (_dashTime > 0f) return PlatformerState.Dash;
            if (_slideTime > 0f) return PlatformerState.Slide;
            if (_grounded)
                return _crouching ? PlatformerState.Crouch
                    : Math.Abs(_velocity.X) > 1f ? PlatformerState.Run : PlatformerState.Idle;
            if (_wallDir != 0 && Math.Sign(h) == _wallDir && _velocity.Y > 0f && Has(MovementAbilities.WallJump))
                return PlatformerState.WallSlide;
            if (_velocity.Y < 0f) return PlatformerState.Jump;
            if (jumpHeld && Has(MovementAbilities.Glide) && _velocity.Y <= _glideFallSpeed + 1f)
                return PlatformerState.Glide;
            return PlatformerState.Fall;
        }

        private void SetCrouch(bool crouch, ref Vector2 pos)
        {
            if (_crouchHalfHeight >= _halfHeight) { _crouching = false; return; }
            if (crouch == _crouching) return;
            // Mantém os pés no lugar ao trocar a altura do corpo.
            float feet = pos.Y + CurrentHalfHeight;
            _crouching = crouch;
            pos.Y = feet - CurrentHalfHeight;
            Owner.Transform.Position = pos;
        }

        private bool CanStand(List<SolidPhysics.Box> solids, Vector2 pos)
        {
            float feet = pos.Y + CurrentHalfHeight;
            var min = new Vector2(pos.X - _halfWidth + 0.5f, feet - _halfHeight * 2f);
            var max = new Vector2(pos.X + _halfWidth - 0.5f, feet - 0.5f);
            foreach (var b in solids)
                if (min.X < b.Max.X && max.X > b.Min.X && min.Y < b.Max.Y && max.Y > b.Min.Y)
                    return false;
            return true;
        }

        private int FindOverlap(List<SolidPhysics.Box> solids, float x, float y, float halfH)
        {
            var min = new Vector2(x - _halfWidth, y - halfH + 0.5f);
            var max = new Vector2(x + _halfWidth, y + halfH - 0.5f);
            for (int i = 0; i < solids.Count; i++)
            {
                var b = solids[i];
                if (min.X < b.Max.X && max.X > b.Min.X && min.Y < b.Max.Y && max.Y > b.Min.Y)
                    return i;
            }
            return -1;
        }

        private int FindBelow(List<SolidPhysics.Box> solids, float x, float y, float halfH)
        {
            float feet = y + halfH;
            for (int i = 0; i < solids.Count; i++)
            {
                var b = solids[i];
                if (x + _halfWidth > b.Min.X && x - _halfWidth < b.Max.X && Math.Abs(b.Min.Y - feet) < 2f)
                    return i;
            }
            return -1;
        }

        private bool Touching(List<SolidPhysics.Box> solids, float x, float y, float halfH)
        {
            var min = new Vector2(x - _halfWidth, y - halfH + 2f);
            var max = new Vector2(x + _halfWidth, y + halfH - 2f);
            foreach (var b in solids)
                if (min.X < b.Max.X && max.X > b.Min.X && min.Y < b.Max.Y && max.Y > b.Min.Y)
                    return true;
            return false;
        }

        /// <summary>Procura uma quina à frente, na altura das mãos, com espaço livre acima.</summary>
        private bool TryGrabLedge(List<SolidPhysics.Box> solids, float x, float y, float halfH, float h,
            out Vector2 hang, out Vector2 standOnTop)
        {
            hang = standOnTop = default;
            int dir = _wallDir != 0 ? _wallDir : (Math.Abs(h) > 0.2f ? Math.Sign(h) : 0);
            if (dir == 0) return false;
            float head = y - halfH;
            foreach (var b in solids)
            {
                bool adjacent = dir > 0
                    ? Math.Abs(b.Min.X - (x + _halfWidth)) < 3f
                    : Math.Abs(b.Max.X - (x - _halfWidth)) < 3f;
                if (!adjacent) continue;
                float top = b.Min.Y;
                if (top < head - 2f || top > head + _ledgeGrabReach) continue;

                // Espaço acima da quina precisa estar livre para subir.
                float cornerX = dir > 0 ? b.Min.X : b.Max.X;
                var stand = new Vector2(cornerX + dir * (_halfWidth + 1f), top - _halfHeight - 0.5f);
                var fmin = new Vector2(stand.X - _halfWidth, stand.Y - _halfHeight);
                var fmax = new Vector2(stand.X + _halfWidth, stand.Y + _halfHeight);
                bool blocked = false;
                foreach (var o in solids)
                    if (fmin.X < o.Max.X && fmax.X > o.Min.X && fmin.Y < o.Max.Y && fmax.Y > o.Min.Y) { blocked = true; break; }
                if (blocked) continue;

                Facing = dir;
                hang = new Vector2(x, top + halfH * 0.35f);
                standOnTop = stand;
                return true;
            }
            return false;
        }

        private void SetGround(GameObject? ground)
        {
            _ground = ground;
            if (ground != null) _groundLastPos = ground.Transform.WorldPosition;
        }

        /// <summary>Plataformas móveis: acompanha o deslocamento do objeto em que está pisando.</summary>
        private void CarryWithGround()
        {
            if (_ground == null || !_grounded || _ground.Scene == null) return;
            var now = _ground.Transform.WorldPosition;
            var delta = now - _groundLastPos;
            if (delta != Vector2.Zero)
                Owner.Transform.Position += delta;
            _groundLastPos = now;
        }

        private void Fire(Action<PlatformerController>? evt, string name)
        {
            evt?.Invoke(this);
            if (FeedAnimator)
                Get<AnimationStateMachine>(Owner)?.SetTrigger(name);
            if (SendMessages)
                Owner.Scene?.Send("platformer:" + name, Owner);
        }

        private void FeedAnim()
        {
            if (!FeedAnimator) return;
            var fsm = Get<AnimationStateMachine>(Owner);
            if (fsm == null) return;
            fsm.SetBool("grounded", _grounded);
            fsm.SetBool("moving", Math.Abs(_currentVx) > 1f);
            fsm.SetBool("falling", !_grounded && _velocity.Y > 0f);
            fsm.SetBool("wallSlide", _state == PlatformerState.WallSlide);
            fsm.SetBool("ledgeHang", _state == PlatformerState.LedgeHang);
            fsm.SetBool("ledgeClimb", _state == PlatformerState.LedgeClimb);
            fsm.SetBool("dashing", _state == PlatformerState.Dash);
            fsm.SetBool("sliding", _state == PlatformerState.Slide);
            fsm.SetBool("crouching", _state == PlatformerState.Crouch);
            fsm.SetBool("gliding", _state == PlatformerState.Glide);
            fsm.SetBool("groundPound", _state == PlatformerState.GroundPound);
        }

        private static T? Get<T>(GameObject obj) where T : SceneComponent
            => obj.Components.OfType<T>().FirstOrDefault();

        /// <summary>Caixas sólidas da cena (exceto as do próprio objeto) e seus donos (null para tiles).</summary>
        private List<SolidPhysics.Box> CollectSolids(Scene? scene, out List<GameObject?> owners)
        {
            var boxes = new List<SolidPhysics.Box>();
            owners = new List<GameObject?>();
            if (scene == null)
                return boxes;

            foreach (var obj in AllObjects(scene.Objects))
            {
                if (obj == Owner)
                    continue;
                foreach (var component in obj.Components)
                {
                    if (!component.Enabled)
                        continue;
                    if (component is BoxCollider collider)
                    {
                        var (min, max) = collider.WorldBounds();
                        boxes.Add(new SolidPhysics.Box(min, max));
                        owners.Add(obj);
                    }
                    else if (component is TilemapRenderer tilemap)
                    {
                        foreach (var (min, max) in tilemap.SolidBoxes())
                        {
                            boxes.Add(new SolidPhysics.Box(min, max));
                            owners.Add(null);
                        }
                    }
                }
            }
            return boxes;
        }

        private static IEnumerable<GameObject> AllObjects(IEnumerable<GameObject> objects)
        {
            foreach (var obj in objects)
            {
                yield return obj;
                foreach (var child in AllObjects(obj.Children))
                    yield return child;
            }
        }
    }
}
