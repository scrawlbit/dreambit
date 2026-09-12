using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace DreamBit.Engine.Rendering
{
    /// <summary>
    /// Cache simples de texturas carregadas de arquivo (PNG etc.), por caminho.
    /// Evita reler o arquivo a cada frame. Vive pelo tempo da aplicação (editor/player).
    /// Suporta hot-reload: <see cref="Invalidate"/> marca o caminho como obsoleto (pode
    /// ser chamado de qualquer thread, ex.: um FileSystemWatcher) e o próximo
    /// <see cref="Get"/> — na thread de desenho — descarta a textura antiga e relê o arquivo.
    /// </summary>
    public static class TextureCache
    {
        private static readonly Dictionary<string, Texture2D?> _cache =
            new(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> _stale =
            new(StringComparer.OrdinalIgnoreCase);
        private static readonly object _gate = new();

        public static Texture2D? Get(GraphicsDevice device, string? path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            // Caminho relativo: resolve ao lado do executável (jogo exportado).
            if (!Path.IsPathRooted(path))
                path = Path.Combine(AppContext.BaseDirectory, path);

            lock (_gate)
            {
                // Marcado como obsoleto por edição externa: descarta aqui (thread de desenho).
                if (_stale.Remove(path) && _cache.TryGetValue(path, out var old))
                {
                    old?.Dispose();
                    _cache.Remove(path);
                }

                if (_cache.TryGetValue(path, out var cached))
                    return cached;
            }

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

            lock (_gate)
                _cache[path] = texture;

            return texture;
        }

        /// <summary>
        /// Marca um caminho como obsoleto para ser recarregado no próximo <see cref="Get"/>.
        /// Seguro para chamar de qualquer thread (não descarta a textura aqui — isso acontece
        /// na thread de desenho, evitando liberar um recurso de GPU fora do contexto gráfico).
        /// </summary>
        public static void Invalidate(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            if (!Path.IsPathRooted(path))
                path = Path.Combine(AppContext.BaseDirectory, path);

            lock (_gate)
            {
                if (_cache.ContainsKey(path))
                    _stale.Add(path);
            }
        }
    }
}
