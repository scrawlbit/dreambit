using System;
using DreamBit.Engine.Components.Custom;
using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Ponto de fixação nomeado num osso ("mao_d", "ponta_espada", "boca"): dá a posição e a
    /// rotação de mundo de um ponto do rig animado, para hitboxes, efeitos, projéteis e armas.
    /// Fica num objeto do rig (o próprio osso ou um filho dele), com deslocamento local opcional.
    /// Consulte por <see cref="SkeletonAnimator.TryGetAttachPoint"/>.
    /// </summary>
    [GameComponent("DreamBit/Animação/Attach Point", Id = "dreambit.attachpoint")]
    public sealed class AttachPoint : SceneComponent
    {
        public override string DisplayName => "Attach Point";

        /// <summary>Nome do ponto. Vazio = nome do objeto.</summary>
        [Tooltip("Nome usado em TryGetAttachPoint (vazio = nome do objeto)")]
        public string PointName { get; set; } = "";

        /// <summary>Deslocamento no espaço local do objeto (ex.: ao longo do osso).</summary>
        [Tooltip("Deslocamento local (no espaço do osso)")]
        public Vector2 Offset { get; set; }

        /// <summary>Ângulo extra em graus, somado à rotação do osso.</summary>
        [Tooltip("Ângulo extra (graus) somado à rotação do osso")]
        public float Angle { get; set; }

        /// <summary>Nome efetivo do ponto.</summary>
        public string ResolvedName => string.IsNullOrWhiteSpace(PointName) ? Owner?.Name ?? string.Empty : PointName.Trim();

        /// <summary>Posição de mundo do ponto.</summary>
        public Vector2 WorldPosition => Vector2.Transform(Offset, Owner.Transform.WorldMatrix);

        /// <summary>Rotação de mundo (radianos), considerando espelhamento por escala negativa.</summary>
        public float WorldRotation => WorldAngle(Owner.Transform.WorldMatrix, MathHelper.ToRadians(Angle));

        /// <summary>Direção de mundo (vetor unitário) do ponto.</summary>
        public Vector2 WorldDirection => new((float)Math.Cos(WorldRotation), (float)Math.Sin(WorldRotation));

        /// <summary>Ângulo de mundo do eixo X local girado por <paramref name="localAngle"/>.</summary>
        public static float WorldAngle(Matrix world, float localAngle)
        {
            var dir = Vector2.TransformNormal(new Vector2((float)Math.Cos(localAngle), (float)Math.Sin(localAngle)), world);
            return dir.LengthSquared() < 1e-12f ? 0f : (float)Math.Atan2(dir.Y, dir.X);
        }
    }
}
