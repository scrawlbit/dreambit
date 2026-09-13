using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Animation
{
    /// <summary>Transform local de um osso num instante (pose).</summary>
    public readonly record struct BonePose(Vector2 Position, float Rotation, Vector2 Scale)
    {
        /// <summary>Interpola linearmente posição/escala e por menor ângulo a rotação.</summary>
        public static BonePose Lerp(BonePose a, BonePose b, float t)
        {
            return new BonePose(
                Vector2.Lerp(a.Position, b.Position, t),
                a.Rotation + MathHelper.WrapAngle(b.Rotation - a.Rotation) * t,
                Vector2.Lerp(a.Scale, b.Scale, t));
        }
    }

    /// <summary>
    /// Um keyframe da animação de esqueleto: um instante (Time, em segundos) com a pose
    /// local de cada osso, indexada pelo nome do osso.
    /// </summary>
    public sealed class PoseKeyframe
    {
        public float Time { get; set; }
        public Dictionary<string, BonePose> Bones { get; }

        public PoseKeyframe(float time) : this(time, new Dictionary<string, BonePose>()) { }

        public PoseKeyframe(float time, Dictionary<string, BonePose> bones)
        {
            Time = time;
            Bones = bones;
        }
    }
}
