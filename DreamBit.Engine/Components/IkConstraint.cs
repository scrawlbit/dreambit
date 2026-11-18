using System;
using System.Linq;
using DreamBit.Engine.Components.Custom;
using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Restrição de IK de dois ossos (coxa+canela, braço+antebraço): gira o osso superior e o
    /// inferior para a ponta do inferior alcançar um alvo — pés no chão em rampas, mãos
    /// agarrando a beirada. Avaliada depois da pose da animação a cada frame (pelo
    /// <see cref="SkeletonAnimator"/> do rig; sem animador, por conta própria no Update).
    /// O alvo é um objeto da cena pelo nome, ou uma posição definida por código
    /// (<see cref="SetTargetPosition"/>). <see cref="Mix"/> mistura com a pose animada.
    /// </summary>
    [GameComponent("DreamBit/Animação/IK Constraint", Id = "dreambit.ik")]
    public sealed class IkConstraint : SceneComponent
    {
        private GameObject? _targetCache;
        private Vector2? _targetOverride;

        public override string DisplayName => "IK Constraint";

        /// <summary>Nome do osso superior (ex.: "coxa_d").</summary>
        [Tooltip("Nome do osso superior (coxa, braço)")]
        public string UpperBone { get; set; } = "";

        /// <summary>Nome do osso inferior (ex.: "canela_d"); a ponta dele vai ao alvo.</summary>
        [Tooltip("Nome do osso inferior (canela, antebraço); a ponta dele vai ao alvo")]
        public string LowerBone { get; set; } = "";

        /// <summary>Nome do objeto alvo (procurado no rig e depois na cena).</summary>
        [Tooltip("Nome do objeto alvo (no rig ou na cena)")]
        public string Target { get; set; } = "";

        /// <summary>Lado para onde a articulação dobra (joelho para frente/para trás).</summary>
        [Tooltip("Lado para onde a articulação (joelho/cotovelo) dobra")]
        public bool BendPositive { get; set; } = true;

        /// <summary>Quanto do IK vale (0 = só animação, 1 = só IK).</summary>
        [Range(0f, 1f)]
        public float Mix { get; set; } = 1f;

        /// <summary>Define o alvo por código (posição de mundo, ex.: ponto do chão de um raycast);
        /// nulo volta a usar o objeto <see cref="Target"/>.</summary>
        public void SetTargetPosition(Vector2? world) => _targetOverride = world;

        /// <summary>Posição de mundo atual do alvo, se houver.</summary>
        public Vector2? TargetPosition
        {
            get
            {
                if (_targetOverride is { } p)
                    return p;
                var obj = ResolveTarget();
                return obj?.Transform.WorldPosition;
            }
        }

        /// <summary>Raiz do rig: o objeto com <see cref="SkeletonAnimator"/> acima (ou este), senão o topo.</summary>
        private GameObject RigRoot()
        {
            GameObject top = Owner;
            for (var o = Owner; o != null; o = o.Parent)
            {
                if (o.Components.OfType<SkeletonAnimator>().Any())
                    return o;
                top = o;
            }
            return top;
        }

        private bool DrivenByAnimator()
        {
            for (var o = Owner; o != null; o = o.Parent)
                if (o.Components.OfType<SkeletonAnimator>().Any(a => a.Enabled))
                    return true;
            return false;
        }

        private static GameObject? FindByName(GameObject root, string name)
        {
            if (root.Name == name)
                return root;
            foreach (var child in root.Children)
            {
                var found = FindByName(child, name);
                if (found != null)
                    return found;
            }
            return null;
        }

        private GameObject? ResolveTarget()
        {
            if (string.IsNullOrWhiteSpace(Target))
                return null;
            if (_targetCache != null && _targetCache.Name == Target && (_targetCache.Scene != null || Owner.Scene == null))
                return _targetCache;

            _targetCache = FindByName(RigRoot(), Target);
            if (_targetCache == null && Owner.Scene != null)
                foreach (var obj in Owner.Scene.Objects)
                    if ((_targetCache = FindByName(obj, Target)) != null)
                        break;
            return _targetCache;
        }

        /// <summary>
        /// Resolve o IK e aplica (girando os dois ossos). Retorna false se faltar osso/alvo.
        /// </summary>
        public bool Solve()
        {
            if (Owner == null || Mix <= 0f)
                return false;
            var root = RigRoot();
            var upper = FindByName(root, UpperBone);
            var lower = FindByName(root, LowerBone);
            if (upper == null || lower == null || upper == lower || TargetPosition is not { } target)
                return false;

            var lowerBone = lower.Components.OfType<Bone>().FirstOrDefault();
            if (lowerBone == null || lowerBone.Length <= 0.0001f)
                return false;

            float mix = Math.Min(1f, Mix);

            // 1) Osso superior: ângulo analítico (lei dos cossenos) para o cotovelo/joelho.
            var a = upper.Transform.WorldPosition;
            var b = lower.Transform.WorldPosition;
            var e = lowerBone.WorldTip;
            float l1 = Vector2.Distance(a, b);
            float l2 = Vector2.Distance(b, e);
            if (l1 < 0.0001f || l2 < 0.0001f)
                return false;

            // Rig espelhado (escala negativa): o lado da dobra espelha junto com o personagem.
            bool bend = Mirrored(upper) ? !BendPositive : BendPositive;
            float desiredUpper = SolveUpperAngle(a, l1, l2, target, bend);
            float currentUpper = Angle(b - a);
            Rotate(upper, MathHelper.WrapAngle(desiredUpper - currentUpper) * mix);

            // 2) Osso inferior: aponta a ponta para o alvo a partir da nova posição.
            b = lower.Transform.WorldPosition;
            e = lowerBone.WorldTip;
            if (Vector2.DistanceSquared(target, b) > 1e-8f)
                Rotate(lower, MathHelper.WrapAngle(Angle(target - b) - Angle(e - b)) * mix);
            return true;
        }

        /// <summary>
        /// Ângulo de mundo (radianos) que o segmento superior (origem → articulação) deve ter
        /// para a cadeia de comprimentos <paramref name="l1"/> + <paramref name="l2"/> alcançar o
        /// alvo; fora de alcance, aponta reto para ele. <paramref name="bendPositive"/> escolhe a
        /// solução em que o osso inferior fica girado positivamente em relação ao superior.
        /// </summary>
        public static float SolveUpperAngle(Vector2 origin, float l1, float l2, Vector2 target, bool bendPositive)
        {
            var toTarget = target - origin;
            float d = toTarget.Length();
            float baseAngle = Angle(toTarget);
            if (d < 1e-5f)
                return baseAngle;

            d = Math.Clamp(d, Math.Abs(l1 - l2) + 1e-4f, l1 + l2);
            float cos = (l1 * l1 + d * d - l2 * l2) / (2f * l1 * d);
            float angle = (float)Math.Acos(Math.Clamp(cos, -1f, 1f));
            return bendPositive ? baseAngle - angle : baseAngle + angle;
        }

        private static float Angle(Vector2 v) => (float)Math.Atan2(v.Y, v.X);

        /// <summary>Gira o objeto <paramref name="worldDelta"/> radianos no mundo (convertendo para
        /// local; pai espelhado inverte o sentido).</summary>
        private static void Rotate(GameObject obj, float worldDelta)
        {
            if (Math.Abs(worldDelta) < 1e-7f)
                return;
            obj.Transform.Rotation += Mirrored(obj) ? -worldDelta : worldDelta;
        }

        /// <summary>True se o pai do objeto está espelhado no mundo (determinante negativo).</summary>
        private static bool Mirrored(GameObject obj)
        {
            if (obj.Parent == null)
                return false;
            var m = obj.Parent.Transform.WorldMatrix;
            return m.M11 * m.M22 - m.M12 * m.M21 < 0f;
        }

        protected internal override void Update(GameTime gameTime)
        {
            // Com SkeletonAnimator no rig, ele resolve depois da pose (evita resolver duas vezes).
            if (!DrivenByAnimator())
                Solve();
        }
    }
}
