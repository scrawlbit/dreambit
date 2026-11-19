using System;
using System.Collections.Generic;
using DreamBit.Engine.Components.Custom;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Dialogue
{
    /// <summary>Avisos curtos no centro da tela (habilidade nova, item, lembrança), em fila. Chame
    /// <see cref="Show"/> de qualquer lugar; um objeto HUD com este componente os desenha.</summary>
    [GameComponent("UI/Toasts", Id = "dreambit.toasts")]
    public sealed class Toasts : SceneComponent
    {
        private static readonly List<(string Title, string Text, float Time)> _queue = new();
        private float _t;

        public override string DisplayName => "Toasts";

        public float Duration { get; set; } = 3.5f;
        public string Font { get; set; } = "";

        public static void Show(string title, string text = "") => _queue.Add((title, text, 0f));

        public static void Clear() => _queue.Clear();

        protected internal override void Update(GameTime gameTime)
        {
            if (_queue.Count == 0) { _t = 0f; return; }
            _t += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_t >= Duration) { _queue.RemoveAt(0); _t = 0f; }
        }

        protected internal override void DrawScreen(ISceneDrawing drawing)
        {
            if (_queue.Count == 0) return;
            var (title, text, _) = _queue[0];
            float a = MathHelper.Clamp(Math.Min(_t / 0.4f, (Duration - _t) / 0.6f), 0f, 1f);
            var size = Fonts.Measure(Font, 44, title);
            var pos = new Vector2(Screen.Width / 2f - size.X / 2f, Screen.Height * 0.22f);
            Fonts.Draw(drawing, Font, 44, title, pos, new Color(255, 214, 150) * a, Color.Black * 0.7f * a, 2f);
            if (!string.IsNullOrEmpty(text))
            {
                var wrapped = Fonts.Wrap(Font, 28, text, Screen.Width * 0.6f);
                var ts = Fonts.Measure(Font, 28, wrapped);
                Fonts.Draw(drawing, Font, 28, wrapped, new Vector2(Screen.Width / 2f - ts.X / 2f, pos.Y + size.Y + 14), new Color(235, 228, 215) * a, Color.Black * 0.6f * a, 1.5f);
            }
        }
    }
}
