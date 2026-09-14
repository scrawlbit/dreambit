using System.Linq;
using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Máquina de estados de animação simples para plataforma: escolhe o clipe do
    /// SkeletonAnimator do próprio objeto conforme o estado do PlatformerController
    /// (no ar → pulo; andando → walk; parado → idle). Equivalente enxuto ao Animator
    /// do Unity / AnimationTree do Godot, orientado a platformer.
    /// </summary>
    public sealed class AnimatorController : SceneComponent
    {
        private string _idleClip = "idle";
        private string _walkClip = "walk";
        private string _jumpClip = "jump";
        private float _blendTime = 0.15f;
        private string? _lastState;

        public override string DisplayName => "Animator Controller";

        public string IdleClip { get => _idleClip; set => Set(ref _idleClip, value ?? string.Empty); }
        public string WalkClip { get => _walkClip; set => Set(ref _walkClip, value ?? string.Empty); }
        public string JumpClip { get => _jumpClip; set => Set(ref _jumpClip, value ?? string.Empty); }

        /// <summary>Tempo de crossfade (transição suave) ao trocar de clipe (0 = corte seco).</summary>
        public float BlendTime { get => _blendTime; set => Set(ref _blendTime, System.Math.Max(0f, value)); }

        protected internal override void OnPlayStarted() => _lastState = null;

        protected internal override void Update(GameTime gameTime)
        {
            var platformer = Owner.Components.OfType<PlatformerController>().FirstOrDefault();
            var skeleton = Owner.Components.OfType<SkeletonAnimator>().FirstOrDefault();
            if (platformer == null || skeleton == null)
                return;

            string clip = !platformer.Grounded ? _jumpClip
                        : platformer.IsMovingHorizontally ? _walkClip
                        : _idleClip;

            if (clip == _lastState || string.IsNullOrEmpty(clip))
                return;

            // Só troca se o clipe existir no rig.
            if (skeleton.ClipNames.Any(n => n == clip))
            {
                skeleton.Play(clip, _blendTime);
                _lastState = clip;
            }
        }
    }
}
