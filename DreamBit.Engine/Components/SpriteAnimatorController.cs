using System.Linq;
using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;
using GameInput = DreamBit.Engine.Input.Input;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Máquina de estados de animação para <see cref="SpriteAnimator"/> (folha de sprites com
    /// clipes nomeados): escolhe o clipe conforme o movimento (no ar → pulo; andando → walk;
    /// parado → idle) e dispara o clipe de ataque uma vez ao pressionar a ação. Também vira o
    /// sprite (flip) pela direção.
    /// Lê o estado de um <see cref="PlatformerController"/> ou de um <see cref="Rigidbody2D"/>.
    /// </summary>
    public sealed class SpriteAnimatorController : SceneComponent
    {
        private string _idleClip = "idle";
        private string _walkClip = "walk";
        private string _jumpClip = "jump";
        private string _attackClip = "attack";
        private string _attackAction = "Action";
        private bool _flipByVelocity = true;
        private bool _artFacesRight = true;
        private float _blendTime = 0.1f;
        private bool _attacking;
        private Vector2 _lastPos;

        public override string DisplayName => "Sprite Animator Controller";

        public string IdleClip { get => _idleClip; set => Set(ref _idleClip, value ?? string.Empty); }
        public string WalkClip { get => _walkClip; set => Set(ref _walkClip, value ?? string.Empty); }
        public string JumpClip { get => _jumpClip; set => Set(ref _jumpClip, value ?? string.Empty); }
        public string AttackClip { get => _attackClip; set => Set(ref _attackClip, value ?? string.Empty); }

        /// <summary>Ação de input que dispara o ataque (mapeada em <see cref="GameInput"/>).</summary>
        public string AttackAction { get => _attackAction; set => Set(ref _attackAction, value ?? string.Empty); }

        /// <summary>Vira o sprite (flip horizontal) conforme a direção do movimento.</summary>
        public bool FlipByVelocity { get => _flipByVelocity; set => Set(ref _flipByVelocity, value); }

        /// <summary>Se a arte olha para a direita por padrão (senão inverte o flip).</summary>
        public bool ArtFacesRight { get => _artFacesRight; set => Set(ref _artFacesRight, value); }

        /// <summary>Tempo de crossfade (dissolve) ao trocar de clipe (0 = corte seco).</summary>
        public float BlendTime { get => _blendTime; set => Set(ref _blendTime, System.Math.Max(0f, value)); }

        /// <summary>True enquanto o clipe de ataque está tocando.</summary>
        public bool IsAttacking => _attacking;

        protected internal override void OnPlayStarted()
        {
            _attacking = false;
            _lastPos = Owner?.Transform.WorldPosition ?? Vector2.Zero;
        }

        protected internal override void Update(GameTime gameTime)
        {
            var animator = Owner?.Components.OfType<SpriteAnimator>().FirstOrDefault();
            if (animator == null)
                return;

            ReadMotion(gameTime, out bool grounded, out float velocityX, out bool moving);

            // Encerra o ataque quando o clipe termina.
            if (_attacking && (animator.CurrentClip != _attackClip || animator.CurrentClipFinished))
                _attacking = false;

            // Dispara o ataque (uma vez), tem prioridade sobre locomoção.
            if (!_attacking && HasClip(animator, _attackClip) &&
                !string.IsNullOrEmpty(_attackAction) && GameInput.JustPressed(_attackAction))
            {
                animator.Play(_attackClip, _blendTime);
                _attacking = true;
            }

            if (!_attacking)
            {
                string clip = !grounded ? _jumpClip : moving ? _walkClip : _idleClip;
                if (HasClip(animator, clip))
                    animator.Play(clip, _blendTime);
            }

            // Vira o sprite pela direção (mantém a direção anterior quando parado).
            if (_flipByVelocity && System.Math.Abs(velocityX) > 1f)
            {
                bool goingRight = velocityX > 0f;
                animator.FlipX = _artFacesRight ? !goingRight : goingRight;
            }
        }

        private void ReadMotion(GameTime gameTime, out bool grounded, out float velocityX, out bool moving)
        {
            var platformer = Owner!.Components.OfType<PlatformerController>().FirstOrDefault();
            if (platformer != null)
            {
                grounded = platformer.Grounded;
                velocityX = platformer.CurrentVelocityX;
                moving = platformer.IsMovingHorizontally;
                _lastPos = Owner.Transform.WorldPosition;
                return;
            }

            var topDown = Owner.Components.OfType<TopDownController>().FirstOrDefault();
            if (topDown != null)
            {
                grounded = true; // top-down não tem pulo
                velocityX = topDown.CurrentVelocity.X;
                moving = topDown.IsMoving;
                _lastPos = Owner.Transform.WorldPosition;
                return;
            }

            var body = Owner.Components.OfType<Rigidbody2D>().FirstOrDefault();
            if (body != null)
            {
                var v = body.LinearVelocity;
                grounded = System.Math.Abs(v.Y) < 8f;
                velocityX = v.X;
                moving = System.Math.Abs(v.X) > 8f;
                _lastPos = Owner.Transform.WorldPosition;
                return;
            }

            // Sem física: deduz do deslocamento do transform.
            var pos = Owner.Transform.WorldPosition;
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var delta = pos - _lastPos;
            _lastPos = pos;
            velocityX = dt > 0f ? delta.X / dt : 0f;
            grounded = dt > 0f ? System.Math.Abs(delta.Y / dt) < 8f : true;
            moving = System.Math.Abs(velocityX) > 8f;
        }

        private static bool HasClip(SpriteAnimator animator, string clip)
            => !string.IsNullOrEmpty(clip) && animator.Clips.Any(c => c.Name == clip);
    }
}
