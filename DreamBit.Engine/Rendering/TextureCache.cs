using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace DreamBit.Engine.Rendering
{
    /// <summary>
    /// Cache simples de texturas carregadas de arquivo (PNG etc.), por caminho.
    /// Evita reler o arquivo a cada frame. Vive pelo tempo da aplicação (editor/player).
    /// </summary>
    public static class TextureCache
    {
        private static readonly Dictionary<string, Texture2D?> _cache = new();

        public static Texture2D? Get(GraphicsDevice device, string? path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            // Caminho relativo: resolve ao lado do executável (jogo exportado).
            if (!Path.IsPathRooted(path))
                path = Path.Combine(AppContext.BaseDirectory, path);

            if (_cache.TryGetValue(path, out var cached))
                return cached;

            Texture2D? texture = null;
            try
            {
                using var stream = File.OpenRead(path);
                texture = Texture2D.FromStream(device, stream);
            }
            catch
            {
                texture = null; // arquivo inexistente/invalido; nao tenta de novo
            }

            _cache[path] = texture;
            return texture;
        }

        /// <summary>Descarta o cache de um caminho (para recarregar após edição externa).</summary>
        public static void Invalidate(string path)
        {
            if (_cache.TryGetValue(path, out var texture))
            {
                texture?.Dispose();
                _cache.Remove(path);
            }
        }
    }
}
