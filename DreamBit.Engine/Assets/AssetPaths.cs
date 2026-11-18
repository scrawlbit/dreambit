using System;
using System.Collections.Generic;
using System.IO;

namespace DreamBit.Engine.Assets
{
    /// <summary>
    /// Resolução única de caminhos de asset relativos (texturas, sons, mapas, scripts). Cenas
    /// guardam caminhos relativos à raiz do projeto; o jogo define <see cref="Roots"/> (ex.: a
    /// pasta "Content" ao lado do executável) e todos os carregadores passam por aqui.
    /// Ordem: raízes registradas → pasta do executável → diretório atual.
    /// </summary>
    public static class AssetPaths
    {
        private static readonly List<string> _roots = new();

        /// <summary>Raízes de busca registradas, em ordem de prioridade.</summary>
        public static IReadOnlyList<string> Roots => _roots;

        /// <summary>Acrescenta uma raiz de busca (ignorada se já existir).</summary>
        public static void AddRoot(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder))
                return;
            var full = Path.GetFullPath(folder);
            if (!_roots.Exists(r => string.Equals(r, full, StringComparison.OrdinalIgnoreCase)))
                _roots.Add(full);
        }

        /// <summary>Remove todas as raízes registradas.</summary>
        public static void ClearRoots() => _roots.Clear();

        /// <summary>Caminho absoluto para <paramref name="path"/>. Caminhos absolutos voltam
        /// inalterados; relativos são procurados nas raízes. Se não existir em lugar nenhum,
        /// devolve o caminho na primeira raiz (ou relativo ao executável).</summary>
        public static string Resolve(string path)
        {
            if (string.IsNullOrEmpty(path) || Path.IsPathRooted(path))
                return path;

            foreach (var root in _roots)
            {
                var candidate = Path.Combine(root, path);
                if (File.Exists(candidate))
                    return candidate;
            }

            var atBase = Path.Combine(AppContext.BaseDirectory, path);
            if (File.Exists(atBase))
                return atBase;

            var atCwd = Path.GetFullPath(path);
            if (File.Exists(atCwd))
                return atCwd;

            return _roots.Count > 0 ? Path.Combine(_roots[0], path) : atBase;
        }
    }
}
