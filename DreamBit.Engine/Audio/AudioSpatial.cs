using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Audio
{
    /// <summary>
    /// Matemática de áudio espacial 2D: atenuação por distância e panorâmica (pan) esquerda↔direita
    /// a partir da posição do ouvinte. Pura e testável, sem depender do motor de áudio.
    /// </summary>
    public static class AudioSpatial
    {
        /// <summary>
        /// Fatores para uma fonte em <paramref name="source"/> ouvida de <paramref name="listener"/>:
        /// atenuação em [0,1] (1 = na posição do ouvinte, 0 = em <paramref name="maxDistance"/> ou além)
        /// e pan em [-1,1] (negativo = à esquerda). Queda linear com a distância.
        /// </summary>
        public static (float Attenuation, float Pan) Compute(Vector2 source, Vector2 listener, float maxDistance)
        {
            if (maxDistance <= 0f)
                return (1f, 0f);

            var delta = source - listener;
            float dist = delta.Length();
            float attenuation = MathHelper.Clamp(1f - dist / maxDistance, 0f, 1f);
            float pan = MathHelper.Clamp(delta.X / maxDistance, -1f, 1f);
            return (attenuation, pan);
        }
    }
}
