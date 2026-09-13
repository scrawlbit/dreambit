using System;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Controlador de plataforma: no play mode aplica gravidade e faz o objeto
    /// pousar sobre as ledges da cena. Com <see cref="UseKeyboard"/>, vira um
    /// personagem jogável (setas/A-D andam; Espaço/W/Cima pulam quando no chão);
    /// senão, usa <see cref="HorizontalSpeed"/> constante. Fecha o laço das ledges.
    /// </summary>
    public sealed class PlatformerController : SceneComponent
    {
        private float _gravity = 1400f;
        private float _halfHeight = 24f;
        private float _halfWidth = 24f;
        private float _horizontalSpeed = 0f;
        private float _moveSpeed = 220f;
        private float _jumpSpeed = 620f;
        private bool _useKeyboard = true;

        private float _velocityY;
        private bool _grounded;

        public override string DisplayName => "Platformer Controller";

        /// <summary>Aceleração da gravidade (px/s²).</summary>
        public float Gravity { get => _gravity; set => Set(ref _gravity, value); }

        /// <summary>Distância do centro até os "pés" do objeto.</summary>
        public float HalfHeight { get => _halfHeight; set => Set(ref _halfHeight, Math.Max(0f, value)); }

        /// <summary>Meia-largura do objeto (para colisão sólida por AABB).</summary>
        public float HalfWidth { get => _halfWidth; set => Set(ref _halfWidth, Math.Max(0f, value)); }

        /// <summary>Velocidade horizontal constante (px/s) quando não é por teclado.</summary>
        public float HorizontalSpeed { get => _horizontalSpeed; set => Set(ref _horizontalSpeed, value); }

        /// <summary>Controlar por teclado (personagem jogável).</summary>
        public bool UseKeyboard { get => _useKeyboard; set => Set(ref _useKeyboard, value); }

        /// <summary>Velocidade ao andar com o teclado (px/s).</summary>
        public float MoveSpeed { get => _moveSpeed; set => Set(ref _moveSpeed, value); }

        /// <summary>Impulso de pulo (px/s).</summary>
        public float JumpSpeed { get => _jumpSpeed; set => Set(ref _jumpSpeed, value); }

        public void Reset()
        {
            _velocityY = 0f;
            _grounded = false;
        }

        protected internal override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (dt <= 0f)
                return;

            var position = Owner.Transform.Position;
            float vx = _horizontalSpeed;

            if (_useKeyboard)
            {
                var keys = Keyboard.GetState();
                vx = 0f;
                if (keys.IsKeyDown(Keys.Left) || keys.IsKeyDown(Keys.A)) vx -= _moveSpeed;
                if (keys.IsKeyDown(Keys.Right) || keys.IsKeyDown(Keys.D)) vx += _moveSpeed;

                bool jump = keys.IsKeyDown(Keys.Space) || keys.IsKeyDown(Keys.Up) || keys.IsKeyDown(Keys.W);
                if (jump && _grounded)
                {
                    _velocityY = -_jumpSpeed;
                    _grounded = false;
                }
            }

            var scene = Owner.Scene;
            var solids = CollectSolids(scene);

            // Movimento horizontal + bloqueio por colisores sólidos.
            float x = position.X + vx * dt;
            x = SolidPhysics.ResolveX(solids, position.X, x, position.Y, _halfWidth, _halfHeight);

            _velocityY += _gravity * dt;
            float newY = position.Y + _velocityY * dt;
            _grounded = false;

            // Colisão sólida vertical (chão/teto por todos os lados).
            var (resolvedY, grounded, ceiling) = SolidPhysics.ResolveY(solids, x, position.Y, newY, _halfWidth, _halfHeight);
            newY = resolvedY;
            if (grounded) { _velocityY = 0f; _grounded = true; }
            else if (ceiling) { _velocityY = 0f; }

            // Ledges (one-way): só pousa caindo e se ainda não estiver no chão de um sólido.
            if (scene != null && _velocityY >= 0f && !_grounded)
            {
                float feetFrom = position.Y + _halfHeight;
                float feetTo = newY + _halfHeight;
                float surface = LedgePhysics.FindLanding(scene.Ledges, x, feetFrom, feetTo);

                if (!float.IsNaN(surface))
                {
                    newY = surface - _halfHeight;
                    _velocityY = 0f;
                    _grounded = true;
                }
            }

            Owner.Transform.Position = new Vector2(x, newY);
        }

        /// <summary>Caixas dos BoxColliders da cena (exceto os do próprio objeto).</summary>
        private System.Collections.Generic.List<SolidPhysics.Box> CollectSolids(Scene? scene)
        {
            var boxes = new System.Collections.Generic.List<SolidPhysics.Box>();
            if (scene == null)
                return boxes;

            foreach (var obj in AllObjects(scene.Objects))
            {
                if (obj == Owner)
                    continue;
                foreach (var component in obj.Components)
                    if (component is BoxCollider collider)
                    {
                        var (min, max) = collider.WorldBounds();
                        boxes.Add(new SolidPhysics.Box(min, max));
                    }
            }
            return boxes;
        }

        private static System.Collections.Generic.IEnumerable<GameObject> AllObjects(
            System.Collections.Generic.IEnumerable<GameObject> objects)
        {
            foreach (var obj in objects)
            {
                yield return obj;
                foreach (var child in AllObjects(obj.Children))
                    yield return child;
            }
        }
    }
}
