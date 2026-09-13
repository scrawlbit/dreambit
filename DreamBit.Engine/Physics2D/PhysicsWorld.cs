using System.Collections.Generic;
using DreamBit.Engine.Components;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using AVector2 = nkast.Aether.Physics2D.Common.Vector2;

namespace DreamBit.Engine.Physics2D
{
    /// <summary>
    /// Mundo de física 2D (corpos rígidos) por cena, sobre o Aether.Physics2D (Box2D em C#).
    /// O motor de física trabalha em metros; o jogo em pixels — a conversão usa
    /// <see cref="PixelsPerMeter"/>. A cena cria um mundo no início do play e o avança a cada
    /// frame; os <see cref="Rigidbody2D"/> registram seus corpos e sincronizam o Transform.
    /// </summary>
    public sealed class PhysicsWorld
    {
        /// <summary>Escala pixel↔metro (a física é instável em coordenadas de pixel grandes).</summary>
        public const float PixelsPerMeter = 64f;

        private readonly World _world;
        private readonly List<Rigidbody2D> _bodies = new();

        public PhysicsWorld(Vector2 gravityPixels)
        {
            _world = new World(ToMeters(gravityPixels));
        }

        internal World Raw => _world;

        /// <summary>Gravidade em pixels/s² (padrão (0, 980), para baixo).</summary>
        public Vector2 Gravity
        {
            get => ToPixels(_world.Gravity);
            set => _world.Gravity = ToMeters(value);
        }

        internal void Register(Rigidbody2D body) => _bodies.Add(body);

        /// <summary>Avança a simulação e sincroniza os Transforms a partir dos corpos.</summary>
        public void Step(float dt)
        {
            if (dt <= 0f)
                return;
            _world.Step(dt);
            foreach (var body in _bodies)
                body.SyncFromBody();
        }

        // ---- conversões pixel ↔ metro ----
        public static AVector2 ToMeters(Vector2 pixels) => new(pixels.X / PixelsPerMeter, pixels.Y / PixelsPerMeter);
        public static Vector2 ToPixels(AVector2 meters) => new(meters.X * PixelsPerMeter, meters.Y * PixelsPerMeter);
        public static float ToMeters(float pixels) => pixels / PixelsPerMeter;
    }
}
