using System;
using System.Collections.Generic;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Zona de gatilho (coletável): no play mode, quando o personagem (objeto com
    /// PlatformerController) encosta, o objeto some. Colisão por sobreposição AABB.
    /// </summary>
    public sealed class TriggerZone : SceneComponent
    {
        private Vector2 _size = new(32, 32);
        private Color _color = new(120, 230, 150);
        private bool _triggered;

        public override string DisplayName => "Trigger Zone";

        public Vector2 Size { get => _size; set => Set(ref _size, value); }
        public Color Color { get => _color; set => Set(ref _color, value); }

        protected internal override void OnPlayStarted() => _triggered = false;

        protected internal override void Update(GameTime gameTime)
        {
            if (_triggered)
                return;

            var scene = Owner.Scene;
            if (scene == null)
                return;

            var player = FindPlayer(scene.Objects);
            if (player == null)
                return;

            if (Overlaps(player))
            {
                _triggered = true;
                Owner.IsVisible = false; // coletado
            }
        }

        protected internal override void Draw(ISceneDrawing drawing)
        {
            drawing.DrawQuad(Owner.Transform.WorldMatrix, _size, _color * 0.35f);
        }

        private bool Overlaps(GameObject player)
        {
            var c1 = Owner.Transform.WorldPosition;
            var h1 = _size / 2f;
            var c2 = player.Transform.WorldPosition;
            var h2 = PlayerHalfSize(player);

            return Math.Abs(c1.X - c2.X) <= h1.X + h2.X
                && Math.Abs(c1.Y - c2.Y) <= h1.Y + h2.Y;
        }

        private static Vector2 PlayerHalfSize(GameObject player)
        {
            foreach (var component in player.Components)
                if (component is SpriteRenderer sprite)
                    return sprite.Size / 2f;
            return new Vector2(24, 24);
        }

        private static GameObject? FindPlayer(IEnumerable<GameObject> objects)
        {
            foreach (var obj in objects)
            {
                foreach (var component in obj.Components)
                    if (component is PlatformerController)
                        return obj;

                var nested = FindPlayer(obj.Children);
                if (nested != null)
                    return nested;
            }
            return null;
        }
    }
}
