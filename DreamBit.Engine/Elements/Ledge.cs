using System;
using System.Collections.Generic;
using DreamBit.Engine.Notification;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Elements
{
    /// <summary>
    /// Uma "ledge" (borda caminhável) do mapa: uma polilinha que define uma superfície
    /// de colisão/chão, no estilo usado no livro de XNA que o Dean Dodrill adotou em
    /// Dust: An Elysian Tail. Quando <see cref="OneWay"/>, o personagem sobe por baixo
    /// e pousa em cima (plataforma de mão única).
    ///
    /// Ledges são geometria do mapa (vivem na <see cref="Scene"/>), não componentes de
    /// um GameObject.
    /// </summary>
    public sealed class Ledge : NotificationObject
    {
        private readonly List<Vector2> _points = new();
        private string _name = "Ledge";
        private bool _oneWay = true;

        public event Action? Changed;

        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        /// <summary>Plataforma de mão única: atravessa por baixo, pousa em cima.</summary>
        public bool OneWay
        {
            get => _oneWay;
            set => Set(ref _oneWay, value);
        }

        public IReadOnlyList<Vector2> Points => _points;

        public void AddPoint(Vector2 point)
        {
            _points.Add(point);
            Changed?.Invoke();
        }

        public void SetPoints(IEnumerable<Vector2> points)
        {
            _points.Clear();
            _points.AddRange(points);
            Changed?.Invoke();
        }

        public void MovePoint(int index, Vector2 point)
        {
            if (index < 0 || index >= _points.Count)
                return;

            _points[index] = point;
            Changed?.Invoke();
        }

        /// <summary>Os segmentos (pares de pontos consecutivos) da polilinha.</summary>
        public IEnumerable<(Vector2 A, Vector2 B)> Segments()
        {
            for (int i = 0; i < _points.Count - 1; i++)
                yield return (_points[i], _points[i + 1]);
        }

        /// <summary>Menor distância de um ponto à polilinha (para hit-test/seleção).</summary>
        public float DistanceTo(Vector2 point)
        {
            if (_points.Count == 0)
                return float.MaxValue;
            if (_points.Count == 1)
                return Vector2.Distance(point, _points[0]);

            float min = float.MaxValue;
            foreach (var (a, b) in Segments())
                min = Math.Min(min, DistancePointSegment(point, a, b));
            return min;
        }

        private static float DistancePointSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            var ab = b - a;
            float lengthSq = ab.LengthSquared();
            if (lengthSq < 1e-6f)
                return Vector2.Distance(p, a);

            float t = MathHelper.Clamp(Vector2.Dot(p - a, ab) / lengthSq, 0f, 1f);
            var projection = a + ab * t;
            return Vector2.Distance(p, projection);
        }
    }
}
