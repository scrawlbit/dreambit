using System;
using System.Collections.Generic;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Zona de gatilho: no play mode detecta, por sobreposição AABB, quando objetos com a
    /// tag alvo (<see cref="TargetTag"/>, ex.: "Player") entram e saem da zona, disparando
    /// os eventos <see cref="Entered"/> e <see cref="Exited"/> (código é primeira classe).
    /// Se <see cref="DestroyOnEnter"/> (padrão), some ao ser tocada — o caso "coletável".
    /// </summary>
    public sealed class TriggerZone : SceneComponent
    {
        private Vector2 _size = new(32, 32);
        private Color _color = new(120, 230, 150);
        private string _targetTag = "Player";
        private bool _destroyOnEnter = true;
        private string _sendOnEnter = string.Empty;

        private readonly HashSet<GameObject> _inside = new();
        private bool _consumed;

        public override string DisplayName => "Trigger Zone";

        public Vector2 Size { get => _size; set => Set(ref _size, value); }
        public Color Color { get => _color; set => Set(ref _color, value); }

        /// <summary>Tag que dispara a zona. Vazio = qualquer objeto com PlatformerController.</summary>
        public string TargetTag { get => _targetTag; set => Set(ref _targetTag, value ?? string.Empty); }

        /// <summary>Se true, a zona some ao ser tocada (coletável). Se false, é persistente.</summary>
        public bool DestroyOnEnter { get => _destroyOnEnter; set => Set(ref _destroyOnEnter, value); }

        /// <summary>Mensagem enviada à cena ao ser tocada (vazio = nenhuma). Barramento de eventos.</summary>
        public string SendOnEnter { get => _sendOnEnter; set => Set(ref _sendOnEnter, value ?? string.Empty); }

        /// <summary>Disparado quando um objeto com a tag alvo entra na zona.</summary>
        public event Action<GameObject>? Entered;

        /// <summary>Disparado quando um objeto que estava na zona sai dela.</summary>
        public event Action<GameObject>? Exited;

        protected internal override void OnPlayStarted()
        {
            _inside.Clear();
            _consumed = false;
        }

        protected internal override void Update(GameTime gameTime)
        {
            if (_consumed)
                return;

            var scene = Owner.Scene;
            if (scene == null)
                return;

            // Saídas: quem estava dentro e não está mais.
            _inside.RemoveWhere(obj =>
            {
                if (obj.Scene == scene && Overlaps(obj))
                    return false;
                Exited?.Invoke(obj);
                return true;
            });

            // Entradas: candidatos que sobrepõem e ainda não estavam dentro.
            foreach (var candidate in Candidates(scene.Objects))
            {
                if (!Overlaps(candidate) || !_inside.Add(candidate))
                    continue;

                Entered?.Invoke(candidate);
                if (!string.IsNullOrEmpty(_sendOnEnter))
                    scene.Send(_sendOnEnter, Owner);

                if (_destroyOnEnter)
                {
                    _consumed = true;
                    Owner.IsVisible = false; // coletado
                    return;
                }
            }
        }

        protected internal override void Draw(ISceneDrawing drawing)
        {
            drawing.DrawQuad(Owner.Transform.WorldMatrix, _size, _color * 0.35f);
        }

        /// <summary>Objetos que podem disparar a zona: os que têm a tag alvo; sem tag definida,
        /// cai no padrão histórico (qualquer objeto com PlatformerController = o "player").</summary>
        private IEnumerable<GameObject> Candidates(IEnumerable<GameObject> objects)
        {
            foreach (var obj in objects)
            {
                if (Matches(obj))
                    yield return obj;

                foreach (var nested in Candidates(obj.Children))
                    yield return nested;
            }
        }

        private bool Matches(GameObject obj)
        {
            if (!string.IsNullOrEmpty(_targetTag) && obj.HasTag(_targetTag))
                return true;

            // Ponte de compatibilidade: sem tag no objeto, o "Player" é quem tem PlatformerController.
            if (!obj.HasTag(_targetTag) && (_targetTag.Length == 0 ||
                string.Equals(_targetTag, "Player", StringComparison.OrdinalIgnoreCase)))
            {
                foreach (var component in obj.Components)
                    if (component is PlatformerController)
                        return true;
            }

            return false;
        }

        private bool Overlaps(GameObject other)
        {
            var c1 = Owner.Transform.WorldPosition;
            var h1 = _size / 2f;
            var c2 = other.Transform.WorldPosition;
            var h2 = HalfSize(other);

            return Math.Abs(c1.X - c2.X) <= h1.X + h2.X
                && Math.Abs(c1.Y - c2.Y) <= h1.Y + h2.Y;
        }

        private static Vector2 HalfSize(GameObject obj)
        {
            foreach (var component in obj.Components)
                if (component is SpriteRenderer sprite)
                    return sprite.Size / 2f;
            return new Vector2(24, 24);
        }
    }
}
