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

        /// <summary>Keyframe exatamente (dentro da tolerância) no tempo informado, ou nulo.</summary>
        public PoseKeyframe? KeyframeAt(float time, float tolerance = 0.0001f)
        {
            PoseKeyframe? best = null;
            float bestDist = float.MaxValue;
            foreach (var k in Keyframes)
            {
                float d = System.Math.Abs(k.Time - time);
                if (d <= tolerance && d < bestDist) { best = k; bestDist = d; }
            }
            return best;
        }

        /// <summary>Cópia profunda (keyframes, extras e eventos).</summary>
        public PoseClip Clone()
        {
            var copy = new PoseClip(Name) { Duration = Duration, Loop = Loop, Easing = Easing };
            foreach (var k in Keyframes)
                copy.Keyframes.Add(k.Clone());
            copy.Events.AddRange(Events);
            return copy;
        }
    }
}
