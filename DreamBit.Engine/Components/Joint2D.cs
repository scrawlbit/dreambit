using System.Linq;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Physics2D;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Joints;
using AVector2 = nkast.Aether.Physics2D.Common.Vector2;

namespace DreamBit.Engine.Components
{
    /// <summary>Tipo de junta entre dois corpos rígidos.</summary>
    public enum Joint2DKind
    {
        /// <summary>Mantém uma distância fixa (haste/corda); com <see cref="Joint2D.Frequency"/> vira mola.</summary>
        Distance,
        /// <summary>Dobradiça: os corpos giram em torno de um pino comum.</summary>
        Revolute,
        /// <summary>Solda: cola os dois corpos (posição e rotação rígidas).</summary>
        Weld
    }

    /// <summary>
    /// Junta de física entre o <see cref="Rigidbody2D"/> deste objeto e outro corpo (por tag),
    /// sobre o Aether.Physics2D. Sem tag, prende num ponto fixo do mundo (âncora estática) — útil
    /// para pêndulo, ponte, corrente, plataforma pendurada. Criada no início do play.
    /// </summary>
    public sealed class Joint2D : SceneComponent
    {
        private Joint2DKind _kind = Joint2DKind.Distance;
        private string _connectedTag = string.Empty;
        private Vector2 _anchor;
        private bool _collideConnected;
        private float _frequency;
        private float _dampingRatio = 0.5f;

        private bool _created;
        private Joint? _joint;

        public override string DisplayName => "Joint 2D";

        public Joint2DKind Kind { get => _kind; set => Set(ref _kind, value); }

        /// <summary>Tag do outro objeto (com Rigidbody2D). Vazio = ponto fixo no mundo.</summary>
        public string ConnectedTag { get => _connectedTag; set => Set(ref _connectedTag, value ?? string.Empty); }

        /// <summary>Deslocamento (px) do ponto de junta em relação a este objeto.</summary>
        public Vector2 Anchor { get => _anchor; set => Set(ref _anchor, value); }

        /// <summary>Se os dois corpos ligados ainda colidem entre si.</summary>
        public bool CollideConnected { get => _collideConnected; set => Set(ref _collideConnected, value); }

        /// <summary>Distância: frequência da mola em Hz (0 = junta rígida). >0 vira mola macia.</summary>
        public float Frequency { get => _frequency; set => Set(ref _frequency, System.Math.Max(0f, value)); }

        /// <summary>Amortecimento da mola (0..1), quando <see cref="Frequency"/> &gt; 0.</summary>
        public float DampingRatio { get => _dampingRatio; set => Set(ref _dampingRatio, value); }

        protected internal override void OnPlayStarted()
        {
            _created = false;
            _joint = null;
        }

        protected internal override void Update(GameTime gameTime)
        {
            // Cria a junta no 1º frame — aí todos os corpos (StartPlay) já existem.
            if (_created)
                return;
            _created = true;
            TryCreate();
        }

        private void TryCreate()
        {
            var scene = Owner?.Scene;
            var rbA = Owner?.Components.OfType<Rigidbody2D>().FirstOrDefault();
            if (scene == null || rbA?.Body == null)
                return;

            var world = scene.Physics;
            Body bodyA = rbA.Body;

            // Ponto de junta (mundo, metros) = posição deste corpo + offset.
            var anchorLocalA = PhysicsWorld.ToMeters(_anchor);
            var worldAnchor = bodyA.Position + anchorLocalA;

            // Corpo B: outro objeto por tag, ou uma âncora estática num ponto fixo.
            Body bodyB;
            if (!string.IsNullOrEmpty(_connectedTag))
            {
                var other = scene.VisibleInDrawOrder()
                    .Where(o => o.HasTag(_connectedTag))
                    .SelectMany(o => o.Components.OfType<Rigidbody2D>())
                    .FirstOrDefault(r => r.Body != null);
                if (other?.Body == null)
                    return;
                bodyB = other.Body;
            }
            else
            {
                bodyB = world.Raw.CreateBody(worldAnchor, 0f, BodyType.Static);
            }

            var anchorLocalB = worldAnchor - bodyB.Position; // ponto comum, local a B

            _joint = _kind switch
            {
                Joint2DKind.Revolute => JointFactory.CreateRevoluteJoint(world.Raw, bodyA, bodyB, anchorLocalA, anchorLocalB),
                Joint2DKind.Weld => JointFactory.CreateWeldJoint(world.Raw, bodyA, bodyB, anchorLocalA, anchorLocalB),
                _ => CreateDistance(world.Raw, bodyA, bodyB)
            };

            if (_joint != null)
                _joint.CollideConnected = _collideConnected;
        }

        private Joint CreateDistance(World world, Body a, Body b)
        {
            // Haste/corda entre os centros dos corpos, comprimento = distância atual.
            var joint = JointFactory.CreateDistanceJoint(world, a, b, AVector2.Zero, AVector2.Zero);
            if (_frequency > 0f)
            {
                joint.Frequency = _frequency;
                joint.DampingRatio = _dampingRatio;
            }
            return joint;
        }
    }
}
