using System;

namespace Scrawlbit
{
    /// <summary>
    /// Helpers de ponto flutuante úteis em jogos (interpolação, clamp, comparação
    /// aproximada). Independente de qualquer engine — serve engine, jogo e scripts.
    /// </summary>
    public static class Mathf
    {
        public const float Epsilon = 1e-5f;

        public static float Clamp(float value, float min, float max)
            => value < min ? min : value > max ? max : value;

        public static float Clamp01(float value) => Clamp(value, 0f, 1f);

        /// <summary>Interpolação linear entre a e b por t (t não é limitado).</summary>
        public static float Lerp(float a, float b, float t) => a + (b - a) * t;

        /// <summary>Interpolação linear limitada (t em [0,1]).</summary>
        public static float LerpClamped(float a, float b, float t) => Lerp(a, b, Clamp01(t));

        /// <summary>Fração de value entre a e b (inverso do Lerp). 0 se a == b.</summary>
        public static float InverseLerp(float a, float b, float value)
            => Math.Abs(b - a) < Epsilon ? 0f : Clamp01((value - a) / (b - a));

        /// <summary>Move de current até target no máximo maxDelta.</summary>
        public static float MoveTowards(float current, float target, float maxDelta)
        {
            if (Math.Abs(target - current) <= maxDelta)
                return target;
            return current + Math.Sign(target - current) * maxDelta;
        }

        /// <summary>True se dois floats são aproximadamente iguais (tolerância relativa).</summary>
        public static bool Approximately(float a, float b)
            => Math.Abs(b - a) <= Epsilon * Math.Max(1f, Math.Max(Math.Abs(a), Math.Abs(b)));

        /// <summary>Repete value no intervalo [0, length) (como o operador módulo, sempre >= 0).</summary>
        public static float Repeat(float value, float length)
        {
            if (length <= 0f) return 0f;
            return value - (float)Math.Floor(value / length) * length;
        }

        /// <summary>Vai e volta entre 0 e length conforme value cresce.</summary>
        public static float PingPong(float value, float length)
        {
            float t = Repeat(value, length * 2f);
            return length - Math.Abs(t - length);
        }
    }
}
