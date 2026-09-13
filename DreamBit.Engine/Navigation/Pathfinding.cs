using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Navigation
{
    /// <summary>
    /// Busca de caminho em grade (A*), para IA de inimigos/NPCs. Trabalha sobre uma grade de
    /// células bloqueadas (ex.: tiles sólidos de um tilemap — ver <see cref="NavGrid"/>) e acha
    /// o menor caminho entre duas células, com movimento em 4 ou 8 direções. Algoritmo puro,
    /// testável. Equivale ao AStarGrid2D do Godot / NavMesh 2D do Unity (em versão de grade).
    /// </summary>
    public static class Pathfinding
    {
        /// <summary>
        /// Acha o caminho de <paramref name="start"/> a <paramref name="goal"/> na grade
        /// <paramref name="blocked"/> (largura×altura). Retorna a lista de células do início ao
        /// fim (inclusive), ou lista vazia se não houver caminho. Diagonais opcionais (sem
        /// "cortar quina" entre dois bloqueios).
        /// </summary>
        public static List<Point> FindPath(bool[,] blocked, Point start, Point goal, bool allowDiagonal = false)
        {
            int w = blocked.GetLength(0), h = blocked.GetLength(1);
            var result = new List<Point>();
            if (!InBounds(start, w, h) || !InBounds(goal, w, h) || blocked[start.X, start.Y] || blocked[goal.X, goal.Y])
                return result;

            var open = new PriorityQueue<Point, float>();
            var cameFrom = new Dictionary<Point, Point>();
            var gScore = new Dictionary<Point, float> { [start] = 0f };
            open.Enqueue(start, Heuristic(start, goal, allowDiagonal));
            var closed = new HashSet<Point>();

            while (open.Count > 0)
            {
                var current = open.Dequeue();
                if (current == goal)
                    return Reconstruct(cameFrom, current);
                if (!closed.Add(current))
                    continue;

                foreach (var (nb, cost) in Neighbors(current, w, h, blocked, allowDiagonal))
                {
                    float tentative = gScore[current] + cost;
                    if (!gScore.TryGetValue(nb, out var g) || tentative < g)
                    {
                        cameFrom[nb] = current;
                        gScore[nb] = tentative;
                        open.Enqueue(nb, tentative + Heuristic(nb, goal, allowDiagonal));
                    }
                }
            }
            return result; // sem caminho
        }

        private static IEnumerable<(Point, float)> Neighbors(Point p, int w, int h, bool[,] blocked, bool diagonal)
        {
            // 4 direções ortogonais.
            var ortho = new[] { new Point(1, 0), new Point(-1, 0), new Point(0, 1), new Point(0, -1) };
            foreach (var d in ortho)
            {
                var n = new Point(p.X + d.X, p.Y + d.Y);
                if (InBounds(n, w, h) && !blocked[n.X, n.Y])
                    yield return (n, 1f);
            }
            if (!diagonal)
                yield break;

            var diag = new[] { new Point(1, 1), new Point(1, -1), new Point(-1, 1), new Point(-1, -1) };
            foreach (var d in diag)
            {
                var n = new Point(p.X + d.X, p.Y + d.Y);
                if (!InBounds(n, w, h) || blocked[n.X, n.Y])
                    continue;
                // não corta quina: os dois ortogonais adjacentes precisam estar livres.
                if (blocked[p.X + d.X, p.Y] || blocked[p.X, p.Y + d.Y])
                    continue;
                yield return (n, 1.41421356f);
            }
        }

        private static float Heuristic(Point a, Point b, bool diagonal)
        {
            int dx = Math.Abs(a.X - b.X), dy = Math.Abs(a.Y - b.Y);
            return diagonal
                ? (dx + dy) + (1.41421356f - 2f) * Math.Min(dx, dy) // octile
                : dx + dy; // manhattan
        }

        private static List<Point> Reconstruct(Dictionary<Point, Point> cameFrom, Point current)
        {
            var path = new List<Point> { current };
            while (cameFrom.TryGetValue(current, out var prev))
            {
                current = prev;
                path.Add(current);
            }
            path.Reverse();
            return path;
        }

        private static bool InBounds(Point p, int w, int h) => p.X >= 0 && p.Y >= 0 && p.X < w && p.Y < h;
    }
}
