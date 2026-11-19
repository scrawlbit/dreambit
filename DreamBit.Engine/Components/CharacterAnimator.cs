using System;
using System.Linq;
using DreamBit.Engine.Components.Custom;
using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Escolhe o clipe do esqueleto pelo estado do PlatformerController (idle, run, jump, fall,
    /// glide…), aceita clipes forçados (ataques, dano) e espelha o personagem pela direção.
    /// </summary>
    [GameComponent("DreamBit/Animação/Character Animator", Id = "dreambit.characteranimator")]
    public sealed class CharacterAnimator : SceneComponent
    {
        private string? _override;
        private float _overrideTime;
        private float _airJumpTime;
        private bool _subscribed;
        private int _facing = 1;

        public override string DisplayName => "Character Animator";

        /// <summary>Tempo de transição entre clipes (s).</summary>
        public float Blend { get; set; } = 0.08f;

        /// <summary>Espelha o personagem pela direção do PlatformerController.</summary>
        public bool FlipWithFacing { get; set; } = true;

        /// <summary>Direção atual (+1 direita, -1 esquerda), também para quem não tem Platformer.</summary>
        public int Facing { get => _facing; set => _facing = value >= 0 ? 1 : -1; }

        /// <summary>Clipe forçado em execução (ataque, dano), ou null.</summary>
        public string? Override => _override;

        /// <summary>Toca um clipe por cima da locomoção por <paramref name="duration"/> s
        /// (≤ 0 = até o clipe terminar).</summary>
        public void Play(string clip, float duration = 0f)
        {
            var anim = Owner.Components.OfType<SkeletonAnimator>().FirstOrDefault();
            if (anim?.GetClip(clip) == null) return;
            _override = clip;
            _overrideTime = duration > 0f ? duration : anim.GetClip(clip)!.Duration;
            anim.Play(clip, Blend * 0.5f, restart: true);
        }

        /// <summary>Clipe permanente (morte): fica até <see cref="ClearOverride"/>.</summary>
        public void Hold(string clip)
        {
            Play(clip);
            _overrideTime = float.MaxValue;
        }

        public void ClearOverride() => _override = null;

        protected internal override void OnPlayStarted()
        {
            _override = null;
            _subscribed = false;
        }

        protected internal override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var anim = Owner.Components.OfType<SkeletonAnimator>().FirstOrDefault();
            if (anim == null) return;
            var pc = Owner.Components.OfType<PlatformerController>().FirstOrDefault();
            if (pc != null && !_subscribed)
            {
                pc.AirJumped += _ => _airJumpTime = 0.35f;
                _subscribed = true;
            }
            _airJumpTime = Math.Max(0f, _airJumpTime - dt);

            if (pc != null) Facing = pc.Facing;
            if (FlipWithFacing)
            {
                var s = Owner.Transform.Scale;
                float sx = Math.Abs(s.X) * Facing;
                if (s.X != sx) Owner.Transform.Scale = new Vector2(sx, s.Y);
            }

            if (_override != null)
            {
                _overrideTime -= dt;
                if (_overrideTime > 0f) return;
                _override = null;
            }

            string clip = pc == null ? "idle" : pc.State switch
            {
                PlatformerState.Run => "run",
                PlatformerState.Jump => _airJumpTime > 0f ? "double_jump" : "jump",
                PlatformerState.Fall => _airJumpTime > 0f ? "double_jump" : "fall",
                PlatformerState.Glide => "glide",
                PlatformerState.WallSlide => "wall_slide",
                PlatformerState.LedgeHang or PlatformerState.LedgeClimb => "ledge_hang",
                PlatformerState.Crouch or PlatformerState.Slide => "crouch",
                PlatformerState.GroundPound => "fall",
                _ => "idle"
            };
            if (anim.GetClip(clip) == null) clip = clip is "double_jump" ? "jump" : clip is "glide" ? "fall" : "idle";
            if (anim.CurrentClipName != clip || (!anim.IsPlaying && anim.Loop))
                anim.Play(clip, Blend);
        }
    }
}
