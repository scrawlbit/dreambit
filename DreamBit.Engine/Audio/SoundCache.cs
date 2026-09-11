using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Audio;

namespace DreamBit.Engine.Audio
{
    /// <summary>Cache de efeitos sonoros (WAV) carregados de arquivo, por caminho.</summary>
    public static class SoundCache
    {
        private static readonly Dictionary<string, SoundEffect?> _cache = new();

        public static SoundEffect? Get(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            if (_cache.TryGetValue(path, out var cached))
                return cached;

            SoundEffect? sound = null;
            try
            {
                using var stream = File.OpenRead(path);
                sound = SoundEffect.FromStream(stream);
            }
            catch
            {
                sound = null; // arquivo invalido/formato nao suportado (só WAV)
            }

            _cache[path] = sound;
            return sound;
        }
    }
}
