using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Rendering
{
    /// <summary>
    /// Detecta frames numa sprite sheet a partir da transparência: agrupa os pixels opacos em
    /// regiões conexas (componentes) e devolve a caixa de cada um — funciona com frames de
    /// tamanhos diferentes e espaçamento irregular (como o "auto-slice" do Aseprite / Sprite
    /// Editor do Unity). Algoritmo puro sobre uma máscara booleana (testável, sem GPU).
    /// </summary>
    public static class FrameDetector
    {
        /// <summary>
        /// Encontra as caixas dos frames. <paramref name="opaque"/> tem width*height itens
        /// (true = pixel visível). Regiões com menos de <paramref name="minPixels"/> pixels
        /// são descartadas (ruído). As caixas voltam em ordem de leitura (linhas de cima para
        /// baixo, esquerda para a direita).
        /// </summary>
        public static List<Rectangle> Detect(bool[] opaque, int width, int height, int minPixels = 16)
        {
            var boxes = new List<Rectangle>();
            if (opaque == null || width <= 0 || height <= 0 || opaque.Length < width * height)
                return boxes;

            var visited = new bool[opaque.Length];
            var stack = new Stack<int>();

            for (int i = 0; i < opaque.Length; i++)
            {
                if (!opaque[i] || visited[i])
                    continue;

                // Flood fill 8-conexo da região a partir deste pixel.
                int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue, count = 0;
                stack.Push(i);
                visited[i] = true;

                while (stack.Count > 0)
                {
                    int p = stack.Pop();
                    int x = p % width, y = p / width;
                    count++;
                    if (x < minX) minX = x;
                    if (y < minY) minY = y;
                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;

                    for (int dy = -1; dy <= 1; dy++)
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            if (dx == 0 && dy == 0) continue;
                            int nx = x + dx, ny = y + dy;
                            if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
                            int np = ny * width + nx;
                            if (opaque[np] && !visited[np])
                            {
                                visited[np] = true;
                                stack.Push(np);
                            }
                        }
                }

                if (count >= minPixels)
                    boxes.Add(new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1));
            }

            return SortReadingOrder(boxes);
        }

        /// <summary>Ordena as caixas em faixas horizontais (linhas) de cima para baixo e, dentro
        /// de cada faixa, da esquerda para a direita — a ordem natural de frames numa folha.</summary>
        private static List<Rectangle> SortReadingOrder(List<Rectangle> boxes)
        {
            boxes.Sort((a, b) => a.Top.CompareTo(b.Top));

            var result = new List<Rectangle>();
            var band = new List<Rectangle>();
            int bandBottom = int.MinValue;

            void FlushBand()
            {
                band.Sort((a, b) => a.Left.CompareTo(b.Left));
                result.AddRange(band);
                band.Clear();
            }

            foreach (var box in boxes)
            {
                // Nova faixa quando a caixa começa abaixo do fim da faixa atual (sem sobreposição vertical).
                if (band.Count > 0 && box.Top > bandBottom)
                {
                    FlushBand();
                    bandBottom = int.MinValue;
                }
                band.Add(box);
                if (box.Bottom > bandBottom)
                    bandBottom = box.Bottom;
            }
            FlushBand();

            return result;
        }
    }
}
