using System;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Messaging;
using DreamBit.Engine.Rendering;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Pós-processamento de tela sem shader: cobre a tela com uma cor (alpha animado) para
    /// fade-in/out entre fases, fade-to-black e flash de dano/impacto. Desenhado por cima do HUD.
    /// Pode ser disparado por mensagem (<see cref="FlashOnMessage"/>). Efeitos com shader custom
    /// (grayscale, bloom) exigem o pipeline MGCB, que fica à parte.
    /// </summary>
    public sealed class ScreenFade : SceneComponent, IMessageReceiver
    {
        private Color _color = Color.Black;
        private float _alpha;
        private float _from, _to, _time, _elapsed;
        private string _flashOnMessage = string.Empty;
        private float _flashDuration = 0.25f;

        public override string DisplayName => "Screen Fade";

        /// <summary>Cor da cobertura (preto para fade de cena; branco/vermelho para flash).</summary>
        public Color Color { get => _color; set => Set(ref _color, value); }

        /// <summary>Opacidade atual [0,1] (0 = transparente).</summary>
        public float Alpha { get => _alpha; set => Set(ref _alpha, MathHelper.Clamp(value, 0f, 1f)); }

        /// <summary>Mensagem que dispara um flash (ex.: "hit"). Vazio = nenhuma.</summary>
        public string FlashOnMessage { get => _flashOnMessage; set => Set(ref _flashOnMessage, value ?? string.Empty); }

        /// <summary>Duração do flash disparado por mensagem.</summary>
        public float FlashDuration { get => _flashDuration; set => Set(ref _flashDuration, Math.Max(0.01f, value)); }

        /// <summary>Escurece a tela (fade-to-color) em <paramref name="duration"/> s.</summary>
        public void FadeOut(float duration) => Animate(_alpha, 1f, duration);

        /// <summary>Clareia a tela (revela a cena) em <paramref name="duration"/> s.</summary>
        public void FadeIn(float duration) => Animate(_alpha, 0f, duration);

        /// <summary>Um flash: sobe ao máximo e volta a zero ao longo de <paramref name="duration"/> s.</summary>
        public void Flash(float duration) { _from = 1f; _to = 0f; _time = Math.Max(0.01f, duration); _elapsed = 0f; _alpha = 1f; }

        private void Animate(float from, float to, float duration)
        {
            _from = from; _to = to; _time = Math.Max(0.001f, duration); _elapsed = 0f;
        }

        void IMessageReceiver.OnMessage(GameMessage message)
        {
            if (!string.IsNullOrEmpty(_flashOnMessage) && message.Name == _flashOnMessage)
                Flash(_flashDuration);
        }

        protected internal override void Update(GameTime gameTime)
        {
            if (_time <= 0f)
                return;
            _elapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;
            float k = MathHelper.Clamp(_elapsed / _time, 0f, 1f);
            Alpha = MathHelper.Lerp(_from, _to, k);
            if (k >= 1f) _time = 0f;
        }

        protected internal override void DrawScreen(ISceneDrawing drawing)
        {
            if (_alpha <= 0.001f)
                return;
            var center = Matrix.CreateTranslation(Screen.Width / 2f, Screen.Height / 2f, 0f);
            drawing.DrawQuad(center, new Vector2(Screen.Width, Screen.Height), _color * _alpha);
        }
    }
}
