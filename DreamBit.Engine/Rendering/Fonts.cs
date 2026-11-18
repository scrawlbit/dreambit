using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DreamBit.Engine.Rendering
{
    /// <summary>
    /// Fontes TrueType (.ttf/.otf) rasterizadas em runtime (FontStashSharp): qualquer tamanho,
    /// acentos e texto nítido para jogos com arte pintada. Registre por nome
    /// (<see cref="Register"/>); componentes de texto usam o nome. Sem fonte registrada, o texto
    /// cai para a <see cref="PixelFont"/>.
    /// </summary>
    public static class Fonts
    {
        private static readonly Dictionary<string, FontSystem> _systems = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Nome da fonte usada quando o componente não define uma.</summary>
        public static string DefaultFont { get; set; } = "default";

        /// <summary>Registra uma fonte a partir de um arquivo (caminho relativo resolvido por AssetPaths).
        /// Vários arquivos no mesmo nome viram fallback (ex.: fonte + fonte de símbolos).</summary>
        public static bool Register(string name, string path)
        {
            var full = Assets.AssetPaths.Resolve(path);
            if (!File.Exists(full))
                return false;
            Register(name, File.ReadAllBytes(full));
            return true;
        }

        /// <summary>Registra uma fonte a partir dos bytes do arquivo .ttf/.otf.</summary>
        public static void Register(string name, byte[] ttf)
        {
            if (!_systems.TryGetValue(name, out var system))
            {
                system = new FontSystem(new FontSystemSettings { FontResolutionFactor = 2f, KernelWidth = 2, KernelHeight = 2 });
                _systems[name] = system;
            }
            system.AddFont(ttf);
        }

        public static bool Has(string? name) => !string.IsNullOrEmpty(name) && _systems.ContainsKey(name);

        /// <summary>Fonte num tamanho (px), ou null se o nome (ou a padrão) não estiver registrado.</summary>
        public static SpriteFontBase? Get(string? name, float size)
        {
            if (string.IsNullOrEmpty(name) || !_systems.TryGetValue(name, out var system))
                if (!_systems.TryGetValue(DefaultFont, out system))
                    return null;
            return system.GetFont(Math.Max(1f, size));
        }

        public static void Clear()
        {
            foreach (var s in _systems.Values) s.Dispose();
            _systems.Clear();
        }

        /// <summary>Mede o texto (já quebrado em linhas por \n).</summary>
        public static Vector2 Measure(string? font, float size, string text)
        {
            var f = Get(font, size);
            if (f != null)
                return f.MeasureString(text);
            int px = PixelScale(size);
            var lines = text.Split('\n');
            int w = 0;
            foreach (var l in lines) w = Math.Max(w, PixelFont.MeasureWidth(l) * px);
            return new Vector2(w, lines.Length * (PixelFont.GlyphHeight + 2) * px);
        }

        /// <summary>Desenha o texto no SpriteBatch atual (já iniciado). Contorno opcional.</summary>
        public static void Draw(ISceneDrawing drawing, string? font, float size, string text, Vector2 position, Color color,
            Color? outline = null, float outlineThickness = 2f)
        {
            if (string.IsNullOrEmpty(text))
                return;
            var f = Get(font, size);
            if (f != null)
            {
                if (outline is { } o)
                {
                    var c = o * (color.A / 255f);
                    for (int dx = -1; dx <= 1; dx++)
                        for (int dy = -1; dy <= 1; dy++)
                            if (dx != 0 || dy != 0)
                                drawing.SpriteBatch.DrawString(f, text, position + new Vector2(dx, dy) * outlineThickness, c);
                }
                drawing.SpriteBatch.DrawString(f, text, position, color);
                return;
            }

            // Sem TTF: fonte de pixel (cada linha).
            int px = PixelScale(size);
            float y = position.Y;
            foreach (var line in text.Split('\n'))
            {
                DrawPixel(drawing, line, (int)position.X, (int)y, px, color);
                y += (PixelFont.GlyphHeight + 2) * px;
            }
        }

        /// <summary>Quebra o texto em linhas que caibam em <paramref name="maxWidth"/> px.</summary>
        public static string Wrap(string? font, float size, string text, float maxWidth)
        {
            if (maxWidth <= 0f || string.IsNullOrEmpty(text))
                return text;
            var sb = new StringBuilder();
            foreach (var paragraph in text.Split('\n'))
            {
                var line = new StringBuilder();
                foreach (var word in paragraph.Split(' '))
                {
                    var candidate = line.Length == 0 ? word : line + " " + word;
                    if (line.Length > 0 && Measure(font, size, candidate).X > maxWidth)
                    {
                        sb.Append(line).Append('\n');
                        line.Clear().Append(word);
                    }
                    else
                    {
                        line.Clear().Append(candidate);
                    }
                }
                sb.Append(line).Append('\n');
            }
            return sb.ToString().TrimEnd('\n');
        }

        private static int PixelScale(float size) => Math.Max(1, (int)Math.Round(size / (PixelFont.GlyphHeight + 2)));

        private static void DrawPixel(ISceneDrawing drawing, string text, int x, int y, int px, Color color)
        {
            int cursor = x;
            foreach (char ch in text)
            {
                var glyph = PixelFont.Glyph(ch);
                for (int row = 0; row < PixelFont.GlyphHeight; row++)
                {
                    byte bits = glyph[row];
                    for (int col = 0; col < PixelFont.GlyphWidth; col++)
                        if (((bits >> (PixelFont.GlyphWidth - 1 - col)) & 1) != 0)
                            drawing.SpriteBatch.Draw(drawing.Pixel, new Rectangle(cursor + col * px, y + row * px, px, px), color);
                }
                cursor += (PixelFont.GlyphWidth + PixelFont.Spacing) * px;
            }
        }
    }
}
