using System.Collections.Generic;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Messaging;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Área que dá golpes: enquanto ativa, aplica dano às <see cref="Hurtbox"/> de outro time que
    /// sobrepõe (uma vez por alvo por ativação). Ative por código (<see cref="Activate"/>) ou por
    /// uma mensagem/evento de animação (<see cref="ActivateOn"/> — ex.: o frame do golpe). Fica
    /// ativa por <see cref="ActiveTime"/> segundos. É o "hitbox" de ataque de jogos de ação.
    /// </summary>
    public sealed class Hitbox : SceneComponent, IMessageReceiver
    {
        private float _width = 60f;
        private float _height = 60f;

        private bool _active;
        private float _timer;
        private readonly HashSet<Health> _hitThisActivation = new();

        public override string DisplayName => "Hitbox";

        public float Width { get => _width; set => Set(ref _width, System.Math.Max(1f, value)); }
        public float Height { get => _height; set => Set(ref _height, System.Math.Max(1f, value)); }
        public Vector2 Offset { get; set; }

        /// <summary>Time do dono (não fere o próprio time).</summary>
        public int Team { get; set; }

        /// <summary>Dano aplicado por golpe.</summary>
        public float Damage { get; set; } = 25f;

        /// <summary>Duração de cada ativação (s).</summary>
        public float ActiveTime { get; set; } = 0.15f;

        /// <summary>Mensagem que ativa a hitbox (ex.: evento de frame do ataque). Vazio = só por código.</summary>
        public string ActivateOn { get; set; } = string.Empty;

        /// <summary>Se está ativa neste instante.</summary>
        public bool IsActive => _active;

        /// <summary>Ativa a hitbox por <see cref="ActiveTime"/> segundos (reinicia os alvos já atingidos).</summary>
        public void Activate()
        {
            _active = true;
            _timer = ActiveTime;
            _hitThisActivation.Clear();
        }

        void IMessageReceiver.OnMessage(GameMessage message)
        {
            if (!string.IsNullOrEmpty(ActivateOn) && message.Name == ActivateOn)
                Activate();
        }

        protected internal override void OnPlayStarted()
        {
            _active = false;
            _timer = 0f;
            _hitThisActivation.Clear();
        }

        protected internal override void Update(GameTime gameTime)
        {
            if (!_active)
                return;

            _timer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            var scene = Owner?.Scene;
            if (scene != null)
            {
                var box = Bounds();
                foreach (var obj in scene.VisibleInDrawOrder())
                    foreach (var component in obj.Components)
                    {
                        if (component is not Hurtbox hurt || hurt.Team == Team || !hurt.Enabled)
                            continue;
                        var health = hurt.Health;
                        if (health == null || _hitThisActivation.Contains(health))
                            continue;
                        if (box.Overlaps(hurt.Bounds()))
                        {
                            health.Damage(Damage);
                            _hitThisActivation.Add(health);
                        }
                    }
            }

            if (_timer <= 0f)
                _active = false;
        }

        private Geometry.Aabb Bounds()
            => Geometry.Aabb.FromCenterSize(Owner.Transform.WorldPosition + Offset, new Vector2(_width, _height));
    }
}
