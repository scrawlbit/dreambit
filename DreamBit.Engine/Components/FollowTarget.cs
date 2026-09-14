using System;
using System.Collections.Generic;
using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Faz o objeto seguir suavemente outro objeto da cena (referência por Id). No play
    /// mode aproxima a posição do alvo a cada frame. Demonstra referências entre objetos.
    /// </summary>
    public sealed class FollowTarget : SceneComponent
    {
        private Guid _targetId;
        private float _speed = 4f;

        public override string DisplayName => "Follow Target";

        /// <summary>Id do objeto a seguir (vazio = nenhum).</summary>
        public Guid TargetId { get => _targetId; set => Set(ref _targetId, value); }

        /// <summary>Rapidez da aproximação (maior = mais rápido).</summary>
        public float Speed { get => _speed; set => Set(ref _speed, Math.Max(0f, value)); }

        protected internal override void Update(GameTime gameTime)
        {
            if (_targetId == Guid.Empty)
                return;

            var scene = Owner.Scene;
            if (scene == null)
                return;

            var target = FindById(scene.Objects, _targetId);
            if (target == null || target == Owner)
                return;

            float t = MathHelper.Clamp(_speed * (float)gameTime.ElapsedGameTime.TotalSeconds, 0f, 1f);
            Owner.Transform.Position = Vector2.Lerp(Owner.Transform.Position, target.Transform.WorldPosition, t);
        }

        private static GameObject? FindById(IEnumerable<GameObject> objects, Guid id)
        {
            foreach (var obj in objects)
            {
                if (obj.Id == id)
                    return obj;

                var nested = FindById(obj.Children, id);
                if (nested != null)
                    return nested;
            }
            return null;
        }
    }
}
