using System;

namespace Scrawlbit
{
    /// <summary>Curvas de suavização (easing) para interpolação de animação/UI.</summary>
    public enum EasingMode { Linear, In, Out, InOut }

    public static class Easing
    {
        /// <summary>Aplica a curva a um fator t em [0,1].</summary>
        public static float Apply(EasingMode mode, float t)
        {
            t = Mathf.Clamp01(t);
            return mode switch
            {
                EasingMode.In => t * t,
                EasingMode.Out => 1f - (1f - t) * (1f - t),
                EasingMode.InOut => t * t * (3f - 2f * t), // smoothstep
                _ => t
            };
        }
    }
}
