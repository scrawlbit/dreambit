using System;
using System.Collections.Generic;
using System.Linq;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Saída de fase: quando um objeto com a tag alvo (ex.: "Player") entra na área,
    /// solicita à cena carregar outra fase (<see cref="TargetScene"/>). É o portal entre
    /// fases — o host (DreamBit.Player) troca de cena.
    /// </summary>
    public sealed class SceneExit : SceneComponent
    {
        private Vector2 _size = new(48, 96);
        private string _targetTag = "Player";
        private string _targetScene = "";
        private bool _triggered;

        public override string DisplayName => "Scene Exit";

        public Vector2 Size { get => _size; set => Set(ref _size, value); }
        public string TargetTag { get => _targetTag; set => Set(ref _targetTag, value ?? string.Empty); }

        /// <summary>Arquivo .dbscene da próxima fase (relativo à pasta da cena atual).</summary>
        public string TargetScene { get => _targetScene; set => Set(ref _targetScene, value ?? string.Empty); }

        protected internal override void OnPlayStarted() => _triggered = false;

        protected internal override void Update(GameTime gameTime)
        {
            if (_triggered || string.IsNullOrEmpty(_targetScene))
                return;

            var scene = Owner.Scene;
            if (scene == null)
                return;

            foreach (var candidate in Candidates(scene.Objects))
            {
                if (!Overlaps(candidate))
                    continue;

                _triggered = true;
                scene.RequestSceneLoad(_targetScene);
                return;
            }
        }

        protected internal override void Draw(ISceneDrawing drawing)
            => drawing.DrawQuad(Owner.Transform.WorldMatrix, _size, new Color(120, 230, 150) * 0.25f);

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
            if (string.Equals(_targetTag, "Player", StringComparison.OrdinalIgnoreCase) && !obj.HasTag(_targetTag))
                return obj.Components.OfType<PlatformerController>().Any();
            return false;
        }

        private bool Overlaps(GameObject other)
        {
            var c1 = Owner.Transform.WorldPosition;
            var h1 = _size / 2f;
            var c2 = other.Transform.WorldPosition;
            var h2 = HalfSize(other);
            return new Geometry.Aabb(c1 - h1, c1 + h1).Intersects(new Geometry.Aabb(c2 - h2, c2 + h2));
        }

        private static Vector2 HalfSize(GameObject obj)
        {
            foreach (var c in obj.Components)
                if (c is SpriteRenderer s)
                    return s.Size / 2f;
            return new Vector2(24, 24);
        }
    }
}
