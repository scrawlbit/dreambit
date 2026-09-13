using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using DreamBit.Engine.Rendering;
using XnaColor = Microsoft.Xna.Framework.Color;

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

        private static readonly Dictionary<string, Bitmap?> _chromaCache = new(StringComparer.Ordinal);

        /// <summary>Versão do bitmap com chroma key aplicado (fundo transparente), para o preview
        /// do editor bater com o runtime. Usa o mesmo algoritmo da engine (<see cref="ChromaKey"/>).</summary>
        public static Bitmap? GetChromaKeyed(string? path, bool auto, XnaColor chroma, int tolerance)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return null;

            string cacheKey = $"{path}|{chroma.PackedValue}|{tolerance}|{(auto ? "a" : "m")}";
            lock (_gate)
                if (_chromaCache.TryGetValue(cacheKey, out var cached))
                    return cached;

            Bitmap? result = null;
            try
            {
                using var src = new Bitmap(path);
                int w = src.PixelSize.Width, h = src.PixelSize.Height;
                int stride = w * 4, size = stride * h;

                var bytes = new byte[size];
                var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                try
                {
                    src.CopyPixels(new PixelRect(0, 0, w, h), handle.AddrOfPinnedObject(), size, stride);
                }
                finally { handle.Free(); }

                // BGRA -> XnaColor(RGBA)
                var colors = new XnaColor[w * h];
                for (int i = 0; i < colors.Length; i++)
                    colors[i] = new XnaColor(bytes[i * 4 + 2], bytes[i * 4 + 1], bytes[i * 4], bytes[i * 4 + 3]);

                var keyColor = auto ? ChromaKey.DetectBackground(colors, w, h) : chroma;
                ChromaKey.Apply(colors, keyColor, tolerance);

                // XnaColor -> BGRA de volta
                for (int i = 0; i < colors.Length; i++)
                {
                    bytes[i * 4] = colors[i].B; bytes[i * 4 + 1] = colors[i].G;
                    bytes[i * 4 + 2] = colors[i].R; bytes[i * 4 + 3] = colors[i].A;
                }

                var wb = new WriteableBitmap(new PixelSize(w, h), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Unpremul);
                using (var fb = wb.Lock())
                    Marshal.Copy(bytes, 0, fb.Address, size);
                result = wb;
            }
            catch
            {
                result = null;
            }

            lock (_gate)
                _chromaCache[cacheKey] = result;
            return result;
        }

        public static void Invalidate(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            lock (_gate)
            {
                if (_cache.Remove(path, out var bitmap))
                    bitmap?.Dispose();
                // invalida as variantes chroma desse caminho
                foreach (var k in new List<string>(_chromaCache.Keys))
                    if (k.StartsWith(path + "|", StringComparison.Ordinal) && _chromaCache.Remove(k, out var cb))
                        cb?.Dispose();
            }
        }
    }
}
