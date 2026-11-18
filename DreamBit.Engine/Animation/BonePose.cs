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

        /// <summary>Matriz local equivalente (mesma ordem do <see cref="Elements.Transform.LocalMatrix"/>).</summary>
        public Matrix ToMatrix() =>
            Matrix.CreateScale(Scale.X, Scale.Y, 1f) *
            Matrix.CreateRotationZ(Rotation) *
            Matrix.CreateTranslation(Position.X, Position.Y, 0f);
    }

    /// <summary>
    /// Um keyframe da animação de esqueleto: um instante (Time, em segundos) com a pose
    /// local de cada osso, indexada pelo nome do osso. Opcionalmente tem uma curva própria
    /// (vale para o segmento até o próximo keyframe, sobrepondo a do clipe) e dados extras
    /// por osso (<see cref="BoneKeys"/>: curva, sprite, ordem, cor e visibilidade).
    /// </summary>
    public sealed class PoseKeyframe
    {
        public float Time { get; set; }
        public Dictionary<string, BonePose> Bones { get; }

        /// <summary>Curva do segmento que começa nesta chave (nulo = easing do clipe).</summary>
        public KeyCurve? Curve { get; set; }

        /// <summary>Extras por osso nesta chave (nome do osso → dados).</summary>
        public Dictionary<string, BoneKey> BoneKeys { get; } = new();

        public PoseKeyframe(float time) : this(time, new Dictionary<string, BonePose>()) { }

        public PoseKeyframe(float time, Dictionary<string, BonePose> bones)
        {
            Time = time;
            Bones = bones;
        }

        /// <summary>Extras do osso nesta chave, ou nulo.</summary>
        public BoneKey? GetBoneKey(string bone) => BoneKeys.TryGetValue(bone, out var k) ? k : null;

        /// <summary>Extras do osso nesta chave (cria se não existir).</summary>
        public BoneKey GetOrAddBoneKey(string bone)
        {
            if (!BoneKeys.TryGetValue(bone, out var k))
                BoneKeys[bone] = k = new BoneKey();
            return k;
        }

        /// <summary>Remove os extras vazios (nenhum campo definido).</summary>
        public void PruneBoneKeys()
        {
            var empty = new List<string>();
            foreach (var (name, key) in BoneKeys)
                if (key.IsEmpty) empty.Add(name);
            foreach (var name in empty)
                BoneKeys.Remove(name);
        }

        /// <summary>Cópia profunda (pose, curva e extras).</summary>
        public PoseKeyframe Clone()
        {
            var copy = new PoseKeyframe(Time, new Dictionary<string, BonePose>(Bones)) { Curve = Curve };
            foreach (var (name, key) in BoneKeys)
                copy.BoneKeys[name] = key.Clone();
            return copy;
        }
    }
}
