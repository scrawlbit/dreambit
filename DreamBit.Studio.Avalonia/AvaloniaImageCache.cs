using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Media.Imaging;
using DreamBit.Engine.Rendering;

namespace DreamBit.Studio.Avalonia
{
    /// <summary>
    /// Cache de <see cref="Bitmap"/> (Avalonia) por caminho, para o SceneView desenhar
    /// texturas reais no canvas. Escuta <see cref="TextureCache.Invalidated"/> para
    /// recarregar no hot-reload (mesmo caminho do cache de textura do MonoGame).
    /// </summary>
    public static class AvaloniaImageCache
    {
        private static readonly Dictionary<string, Bitmap?> _cache = new(StringComparer.OrdinalIgnoreCase);
        private static readonly object _gate = new();

        static AvaloniaImageCache()
        {
            TextureCache.Invalidated += Invalidate;
        }

        /// <summary>Bitmap do caminho (carrega e cacheia); null se o arquivo não existe/é inválido.</summary>
        public static Bitmap? Get(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            lock (_gate)
                if (_cache.TryGetValue(path, out var cached))
                    return cached;

            Bitmap? bitmap = null;
            try
            {
                if (File.Exists(path))
                    bitmap = new Bitmap(path);
            }
            catch
            {
                bitmap = null; // arquivo inválido/formato não suportado
            }

            lock (_gate)
                _cache[path] = bitmap;

            return bitmap;
        }

        public static void Invalidate(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            lock (_gate)
                if (_cache.Remove(path, out var bitmap))
                    bitmap?.Dispose();
        }
    }
}
