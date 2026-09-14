using System.Collections.Generic;
using System.Linq;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Navigation;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// IA de perseguição por pathfinding: recalcula periodicamente um caminho A* (via
    /// <see cref="NavGrid"/> montada dos tiles sólidos da cena) até o objeto com a tag alvo e
    /// caminha pelos waypoints. Sem tilemap sólido, persegue em linha reta. Componente pronto
    /// para inimigos/NPCs.
    /// </summary>
    public sealed class NavChaser : SceneComponent
    {
        private string _targetTag = "player";
        private float _speed = 120f;
        private float _repathInterval = 0.4f;
        private float _arriveRadius = 10f;
        private bool _allowDiagonal = true;

        private NavGrid? _grid;
        private bool _triedGrid;
        private readonly List<Vector2> _path = new();
        private int _waypoint;
        private double _timer;

        public override string DisplayName => "Nav Chaser";

        public string TargetTag { get => _targetTag; set => Set(ref _targetTag, value ?? string.Empty); }
        public float Speed { get => _speed; set => Set(ref _speed, value); }
        public float RepathInterval { get => _repathInterval; set => Set(ref _repathInterval, System.Math.Max(0.05f, value)); }
        public float ArriveRadius { get => _arriveRadius; set => Set(ref _arriveRadius, System.Math.Max(1f, value)); }
        public bool AllowDiagonal { get => _allowDiagonal; set => Set(ref _allowDiagonal, value); }

        /// <summary>Waypoints do caminho atual (mundo), para depurar/testar.</summary>
        public IReadOnlyList<Vector2> CurrentPath => _path;

        protected internal override void OnPlayStarted()
        {
            _grid = null;
            _triedGrid = false;
            _path.Clear();
            _waypoint = 0;
            _timer = _repathInterval; // recalcula no primeiro frame
        }

        protected internal override void Update(GameTime gameTime)
        {
            var scene = Owner?.Scene;
            if (scene == null)
                return;

            var target = FindTarget(scene);
            if (target == null)
                return;

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _timer += dt;
            if (_timer >= _repathInterval)
            {
                _timer = 0;
                Repath(scene, Owner!.Transform.WorldPosition, target.Transform.WorldPosition);
            }

            MoveAlong(Owner!, target.Transform.WorldPosition, dt);
        }

        private GameObject? FindTarget(Scene scene)
            => scene.VisibleInDrawOrder().FirstOrDefault(o => o.HasTag(_targetTag));

        private void Repath(Scene scene, Vector2 from, Vector2 to)
        {
            if (!_triedGrid)
            {
                _triedGrid = true;
                var tilemap = scene.VisibleInDrawOrder()
                    .SelectMany(o => o.Components)
                    .OfType<TilemapRenderer>()
                    .FirstOrDefault(t => t.Solid);
                if (tilemap != null)
                    _grid = NavGrid.FromTilemap(tilemap);
            }

            _path.Clear();
            _waypoint = 0;
            if (_grid != null)
            {
                var pts = _grid.FindPath(from, to, _allowDiagonal);
                // Ignora o primeiro ponto (célula atual) para não travar no lugar.
                for (int i = pts.Count > 1 ? 1 : 0; i < pts.Count; i++)
                    _path.Add(pts[i]);
            }
        }

        private void MoveAlong(GameObject owner, Vector2 targetPos, float dt)
        {
            Vector2 goal;
            if (_waypoint < _path.Count)
            {
                goal = _path[_waypoint];
                if (Vector2.Distance(owner.Transform.WorldPosition, goal) <= _arriveRadius)
                {
                    _waypoint++;
                    return;
                }
            }
            else
            {
                goal = targetPos; // sem caminho: vai direto ao alvo
            }

            var pos = owner.Transform.WorldPosition;
            var dir = goal - pos;
            float len = dir.Length();
            if (len < 0.001f)
                return;

            float step = System.Math.Min(_speed * dt, len);
            owner.Transform.Position += dir / len * step;
        }
    }
}
