using DreamBit.Engine.Elements;
using DreamBit.Engine.Physics2D;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using AVector2 = nkast.Aether.Physics2D.Common.Vector2;

namespace DreamBit.Engine.Components
{
    /// <summary>Tipo de corpo rígido (espelha o BodyType do Box2D/Aether).</summary>
    public enum RigidbodyKind { Static, Dynamic, Kinematic }

    /// <summary>Forma do colisor do corpo.</summary>
    public enum ColliderShape { Box, Circle }

    /// <summary>
    /// Corpo rígido 2D: dá ao objeto física real (gravidade, colisão com rotação, empilhamento,
    /// impulsos) via Aether.Physics2D — alternativa ao PlatformerController. No play, cria um
    /// corpo no mundo da cena e passa a dirigir o Transform pela simulação. Formas: caixa ou
    /// círculo. Scripts podem ler/definir velocidade e aplicar força/impulso.
    /// </summary>
    public sealed class Rigidbody2D : SceneComponent
    {
        private RigidbodyKind _kind = RigidbodyKind.Dynamic;
        private ColliderShape _shape = ColliderShape.Box;
        private float _width = 48f;
        private float _height = 48f;
        private float _radius = 24f;
        private float _density = 1f;
        private float _friction = 0.3f;
        private float _restitution = 0f;
        private bool _fixedRotation;
        private int _category = 1;
        private int _mask = -1;

        private Body? _body;

        public override string DisplayName => "Rigidbody 2D";

        public RigidbodyKind Kind { get => _kind; set => Set(ref _kind, value); }
        public ColliderShape Shape { get => _shape; set => Set(ref _shape, value); }
        public float Width { get => _width; set => Set(ref _width, value < 1f ? 1f : value); }
        public float Height { get => _height; set => Set(ref _height, value < 1f ? 1f : value); }
        public float Radius { get => _radius; set => Set(ref _radius, value < 1f ? 1f : value); }
        public float Density { get => _density; set => Set(ref _density, value < 0f ? 0f : value); }
        public float Friction { get => _friction; set => Set(ref _friction, value); }
        /// <summary>Elasticidade (0 = não quica, 1 = quica totalmente).</summary>
        public float Restitution { get => _restitution; set => Set(ref _restitution, value); }
        /// <summary>Impede o corpo de girar (útil para personagens).</summary>
        public bool FixedRotation { get => _fixedRotation; set => Set(ref _fixedRotation, value); }

        /// <summary>Categorias de colisão às quais este corpo pertence (bits). Padrão 1.</summary>
        public int CollisionCategory { get => _category; set => Set(ref _category, value); }
        /// <summary>Categorias com que este corpo colide (máscara de bits). Padrão -1 (todas).</summary>
        public int CollidesWith { get => _mask; set => Set(ref _mask, value); }

        /// <summary>Velocidade linear em pixels/s (para scripts).</summary>
        public Vector2 LinearVelocity
        {
            get => _body != null ? PhysicsWorld.ToPixels(_body.LinearVelocity) : Vector2.Zero;
            set { if (_body != null) _body.LinearVelocity = PhysicsWorld.ToMeters(value); }
        }

        public void ApplyForce(Vector2 force)
            => _body?.ApplyForce(PhysicsWorld.ToMeters(force));

        public void ApplyImpulse(Vector2 impulse)
            => _body?.ApplyLinearImpulse(PhysicsWorld.ToMeters(impulse));

        protected internal override void OnPlayStarted()
        {
            var scene = Owner?.Scene;
            if (scene == null)
                return;

            var world = scene.Physics;
            var pos = PhysicsWorld.ToMeters(Owner!.Transform.Position);
            _body = world.Raw.CreateBody(pos, Owner.Transform.Rotation, MapKind());
            _body.FixedRotation = _fixedRotation;
            _body.Tag = this; // permite recuperar o Rigidbody2D a partir do corpo (ex.: raycast)

            Fixture fixture = _shape == ColliderShape.Circle
                ? _body.CreateCircle(PhysicsWorld.ToMeters(_radius), _density)
                : _body.CreateRectangle(PhysicsWorld.ToMeters(_width), PhysicsWorld.ToMeters(_height), _density, AVector2.Zero);
            fixture.Friction = _friction;
            fixture.Restitution = _restitution;
            fixture.CollisionCategories = (Category)_category;
            fixture.CollidesWith = (Category)_mask;

            world.Register(this);
        }

        /// <summary>Copia posição/rotação do corpo simulado para o Transform (chamado pós-step).</summary>
        internal void SyncFromBody()
        {
            if (_body == null)
                return;
            Owner.Transform.Position = PhysicsWorld.ToPixels(_body.Position);
            Owner.Transform.Rotation = _body.Rotation;
        }

        private BodyType MapKind() => _kind switch
        {
            RigidbodyKind.Static => BodyType.Static,
            RigidbodyKind.Kinematic => BodyType.Kinematic,
            _ => BodyType.Dynamic
        };
    }
}
