using System.Collections.Generic;
using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;
using GameInput = DreamBit.Engine.Input.Input;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Navegação de UI por teclado/gamepad: move o foco entre os controles focáveis da cena
    /// (setas/D-pad, espacialmente) e confirma (Enter/A → <see cref="IUiFocusable.Activate"/>);
    /// esquerda/direita ajustam um slider focado. Adicione um por cena (como o EventSystem do
    /// Unity). O ponteiro (mouse/toque) continua funcionando em paralelo.
    /// </summary>
    public sealed class UiNavigator : SceneComponent
    {
        private bool _autoFocusFirst = true;

        public override string DisplayName => "UI Navigator";

        /// <summary>Foca o primeiro controle automaticamente ao começar (para gamepad/teclado).</summary>
        public bool AutoFocusFirst { get => _autoFocusFirst; set => Set(ref _autoFocusFirst, value); }

        protected internal override void Update(GameTime gameTime)
        {
            var scene = Owner?.Scene;
            if (scene == null)
                return;

            var list = Collect(scene);
            if (list.Count == 0)
                return;

            var current = UiFocus.Current;
            if ((current == null || !list.Contains(current)) && _autoFocusFirst)
            {
                UiFocus.Set(list[0]);
                current = list[0];
            }
            if (current == null)
                return;

            if (GameInput.JustPressed("MoveUp")) Move(list, current, 0, -1);
            else if (GameInput.JustPressed("MoveDown")) Move(list, current, 0, 1);
            else if (GameInput.JustPressed("MoveLeft")) { if (current is UiSlider s) s.Nudge(-1); else Move(list, current, -1, 0); }
            else if (GameInput.JustPressed("MoveRight")) { if (current is UiSlider s) s.Nudge(1); else Move(list, current, 1, 0); }

            if (GameInput.JustPressed("Action"))
                UiFocus.Current?.Activate();
        }

        private static void Move(IReadOnlyList<IUiFocusable> list, IUiFocusable from, int dx, int dy)
        {
            var best = PickNext(list, from, dx, dy);
            if (best != null)
                UiFocus.Set(best);
        }

        /// <summary>Escolhe o próximo controle na direção (dx,dy) a partir de <paramref name="from"/>,
        /// espacialmente (menor distância na direção + penalidade perpendicular). Público para testes.</summary>
        public static IUiFocusable? PickNext(IReadOnlyList<IUiFocusable> list, IUiFocusable from, int dx, int dy)
        {
            var origin = Center(from.FocusRect);
            IUiFocusable? best = null;
            float bestScore = float.MaxValue;

            foreach (var candidate in list)
            {
                if (ReferenceEquals(candidate, from))
                    continue;
                var c = Center(candidate.FocusRect);
                float ax = c.X - origin.X, ay = c.Y - origin.Y;
                float along = dx != 0 ? ax * dx : ay * dy; // precisa estar na direção pedida
                if (along <= 0f)
                    continue;
                float across = dx != 0 ? System.Math.Abs(ay) : System.Math.Abs(ax);
                float score = along + across * 2f; // penaliza desvio perpendicular
                if (score < bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }
            return best;
        }

        private static Vector2 Center(Rectangle r) => new(r.X + r.Width / 2f, r.Y + r.Height / 2f);

        private static List<IUiFocusable> Collect(Scene scene)
        {
            var list = new List<IUiFocusable>();
            foreach (var obj in scene.VisibleInDrawOrder())
                foreach (var component in obj.Components)
                    if (component is IUiFocusable f && f.Focusable)
                        list.Add(f);
            return list;
        }
    }
}
