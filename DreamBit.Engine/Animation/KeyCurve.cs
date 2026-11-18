using System;
using Scrawlbit;

namespace DreamBit.Engine.Animation
{
    /// <summary>Tipo de curva de interpolação de um keyframe até o próximo.</summary>
    public enum KeyCurveType
    {
        /// <summary>Curva pronta do Scrawlbit (Linear, In, Out, InOut).</summary>
        Easing,
        /// <summary>Segura o valor da chave até a próxima (sem interpolar) — "hold" do Spriter.</summary>
        Step,
        /// <summary>Bézier cúbica (estilo CSS cubic-bezier) com dois pontos de controle.</summary>
        Bezier
    }

    /// <summary>
    /// Curva de interpolação de um segmento de animação (da chave em que está até a próxima).
    /// Pode ser uma suavização pronta, "step" (segura) ou bézier cúbica com dois pontos de
    /// controle (X1,Y1) e (X2,Y2); os extremos são fixos em (0,0) e (1,1), como no CSS.
    /// </summary>
    public readonly record struct KeyCurve(KeyCurveType Type, EasingMode Easing = EasingMode.Linear,
        float X1 = 0f, float Y1 = 0f, float X2 = 1f, float Y2 = 1f)
    {
        /// <summary>Interpolação linear.</summary>
        public static KeyCurve Linear => new(KeyCurveType.Easing, EasingMode.Linear);

        /// <summary>Segura o valor até a próxima chave.</summary>
        public static KeyCurve Step => new(KeyCurveType.Step);

        /// <summary>Curva a partir de uma suavização pronta.</summary>
        public static KeyCurve FromEasing(EasingMode mode) => new(KeyCurveType.Easing, mode);

        /// <summary>Bézier cúbica (X dos controles limitado a [0,1] para a curva ser função do tempo).</summary>
        public static KeyCurve Bezier(float x1, float y1, float x2, float y2)
            => new(KeyCurveType.Bezier, EasingMode.Linear, Mathf.Clamp01(x1), y1, Mathf.Clamp01(x2), y2);

        /// <summary>Aplica a curva a um fator de tempo <paramref name="t"/> em [0,1].</summary>
        public float Evaluate(float t)
        {
            t = Mathf.Clamp01(t);
            return Type switch
            {
                KeyCurveType.Step => t >= 1f ? 1f : 0f,
                KeyCurveType.Bezier => EvaluateBezier(t),
                _ => Scrawlbit.Easing.Apply(Easing, t)
            };
        }

        private float EvaluateBezier(float t)
        {
            if (t <= 0f) return 0f;
            if (t >= 1f) return 1f;

            float x1 = Mathf.Clamp01(X1), x2 = Mathf.Clamp01(X2);

            // Resolve s tal que x(s) = t: Newton-Raphson, com bisseção de reserva.
            float s = t;
            for (int i = 0; i < 8; i++)
            {
                float x = Cubic(x1, x2, s) - t;
                if (MathF.Abs(x) < 1e-6f)
                    return Cubic(Y1, Y2, s);
                float dx = CubicDerivative(x1, x2, s);
                if (MathF.Abs(dx) < 1e-6f)
                    break;
                s -= x / dx;
                if (s < 0f || s > 1f)
                    break;
            }

            float lo = 0f, hi = 1f;
            s = t;
            for (int i = 0; i < 40; i++)
            {
                float x = Cubic(x1, x2, s);
                if (MathF.Abs(x - t) < 1e-6f)
                    break;
                if (x < t) lo = s; else hi = s;
                s = (lo + hi) * 0.5f;
            }
            return Cubic(Y1, Y2, s);
        }

        // Bézier cúbica 1D com P0 = 0 e P3 = 1.
        private static float Cubic(float p1, float p2, float s)
        {
            float u = 1f - s;
            return 3f * u * u * s * p1 + 3f * u * s * s * p2 + s * s * s;
        }

        private static float CubicDerivative(float p1, float p2, float s)
        {
            float u = 1f - s;
            return 3f * u * u * p1 + 6f * u * s * (p2 - p1) + 3f * s * s * (1f - p2);
        }
    }
}
