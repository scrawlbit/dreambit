using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Audio;

namespace DreamBit.Engine.Audio
{
    /// <summary>Cache de efeitos sonoros (WAV) carregados de arquivo, por caminho.</summary>
    public static class SoundCache
    {
        private static readonly Dictionary<string, SoundEffect?> _cache =
            new(StringComparer.OrdinalIgnoreCase);
        private static readonly object _gate = new();

        public static SoundEffect? Get(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            if (!Path.IsPathRooted(path))
                path = Path.Combine(AppContext.BaseDirectory, path);

            lock (_gate)
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

            lock (_gate)
                _cache[path] = sound;

            return sound;
        }

        /// <summary>Descarta o cache de um caminho para recarregar após edição externa (hot-reload).</summary>
        public static void Invalidate(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            if (!Path.IsPathRooted(path))
                path = Path.Combine(AppContext.BaseDirectory, path);

            lock (_gate)
                if (_cache.Remove(path, out var sound))
                    sound?.Dispose();
        }
    }
}
