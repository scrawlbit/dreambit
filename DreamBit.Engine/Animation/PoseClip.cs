using System.Collections.Generic;

namespace DreamBit.Engine.Animation
{
    /// <summary>
    /// Um clipe nomeado de animação de esqueleto (ex.: "idle", "walk"): keyframes de pose,
    /// duração, loop, easing e eventos por tempo. Um SkeletonAnimator tem vários clipes e
    /// um deles é o ativo.
    /// </summary>
    public sealed class PoseClip
    {
        public PoseClip(string name) => Name = name;

        public string Name { get; set; }
        public float Duration { get; set; } = 1f;
        public bool Loop { get; set; } = true;
        public Scrawlbit.EasingMode Easing { get; set; } = Scrawlbit.EasingMode.Linear;

        /// <summary>Keyframes ordenados por Time.</summary>
        public List<PoseKeyframe> Keyframes { get; } = new();

        /// <summary>Eventos por tempo (tempo → nome), ordenados por tempo.</summary>
        public List<(float Time, string Name)> Events { get; } = new();
    }
}
