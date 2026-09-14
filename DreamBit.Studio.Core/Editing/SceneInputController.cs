using System;
using System.Collections.Generic;
using System.Linq;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using DreamBit.Studio.ViewModels;
using Microsoft.Xna.Framework;

namespace DreamBit.Studio.Editing
{
    /// <summary>
    /// Input do canvas na ferramenta de seleção: seleção única/múltipla (Ctrl+clique,
    /// caixa), mover/rotacionar/redimensionar em grupo pelos gizmos, pan e zoom.
    /// </summary>
    public sealed class SceneInputController
    {
        private readonly EditorViewModel _editor;

        private bool _movingGroup;
        private bool _rotating;
        private bool _scaling;
        private bool _panning;
        private bool _boxSelecting;

        private GameObject[] _groupObjects = Array.Empty<GameObject>();
        private EditorViewModel.TransformState[] _groupBefore = Array.Empty<EditorViewModel.TransformState>();
        private Vector2 _groupCenter;
        private Vector2 _dragAnchor;
        private float _rotateStartAngle;
        private float _scaleStartDist;

        private Vector2 _lastScreen;
        private Vector2 _boxStart;
        private Vector2 _boxCurrent;

        public SceneInputController(EditorViewModel editor) => _editor = editor;

        /// <summary>Caixa de seleção em andamento (mundo), ou null.</summary>
        public (Vector2 Min, Vector2 Max)? BoxSelectWorld =>
            _boxSelecting ? (Vector2.Min(_boxStart, _boxCurrent), Vector2.Max(_boxStart, _boxCurrent)) : null;

        public void PrimaryDown(Vector2 screen, int width, int height, bool ctrl)
        {
            var world = _editor.Camera.ScreenToWorld(screen, width, height);
            var selected = _editor.SelectedObjects;
            float zoom = _editor.Camera.Zoom;
            float grab = GizmoGeometry.GrabScreenRadius / zoom;

            // 1) Gizmos da seleção atual (rotação/escala) têm prioridade
            if (selected.Count > 0)
            {
                var (center, handle, corners) = Gizmo(selected, zoom);

                if (Vector2.Distance(world, handle) <= grab)
                {
                    BeginGroupOp(center);
                    _rotating = true;
                    _rotateStartAngle = Angle(center, world);
                    return;
                }

                foreach (var corner in corners)
                {
                    if (Vector2.Distance(world, corner) <= grab)
                    {
                        BeginGroupOp(center);
                        _scaling = true;
                        _scaleStartDist = Math.Max(1f, Vector2.Distance(center, world));
                        return;
                    }
                }
            }

            // 2) Pick de objeto
            var hit = Pick(world);
            if (hit != null)
            {
                if (ctrl)
                {
                    _editor.ToggleSelect(hit);
                    return;
                }

                if (!hit.IsSelected)
                    _editor.SelectSingle(hit);

                BeginGroupOp(Vector2.Zero);
                _movingGroup = true;
                _dragAnchor = world;
                return;
            }

            // 3) Espaço vazio: tenta selecionar uma ledge; senão, seleção por caixa
            if (!ctrl)
                _editor.SelectSingle(null);

            if (_editor.TrySelectLedgeAt(world, 8f / zoom))
                return;

            _boxSelecting = true;
            _boxStart = world;
            _boxCurrent = world;
        }

        public void MiddleDown(Vector2 screen)
        {
            _panning = true;
            _lastScreen = screen;
        }

        public void Move(Vector2 screen, int width, int height)
        {
            var world = _editor.Camera.ScreenToWorld(screen, width, height);

            if (_rotating)
            {
                float delta = Angle(_groupCenter, world) - _rotateStartAngle;
                for (int i = 0; i < _groupObjects.Length; i++)
                {
                    var before = _groupBefore[i];
                    var offset = Rotate(before.Position - _groupCenter, delta);
                    _groupObjects[i].Transform.Position = _groupCenter + offset;
                    _groupObjects[i].Transform.Rotation = before.Rotation + delta;
                }
                return;
            }

            if (_scaling)
            {
                float factor = MathHelper.Clamp(Vector2.Distance(_groupCenter, world) / _scaleStartDist, 0.05f, 100f);
                for (int i = 0; i < _groupObjects.Length; i++)
                {
                    var before = _groupBefore[i];
                    _groupObjects[i].Transform.Position = _groupCenter + (before.Position - _groupCenter) * factor;
                    _groupObjects[i].Transform.Scale = before.Scale * factor;
                }
                return;
            }

            if (_movingGroup)
            {
                var delta = world - _dragAnchor;
                if (_groupObjects.Length == 1 && _editor.SnapToGrid && _editor.GridStep > 0)
                {
                    var target = _groupBefore[0].Position + delta;
                    float step = _editor.GridStep;
                    _groupObjects[0].Transform.Position = new Vector2(
                        (float)Math.Round(target.X / step) * step,
                        (float)Math.Round(target.Y / step) * step);
                }
                else
                {
                    for (int i = 0; i < _groupObjects.Length; i++)
                        _groupObjects[i].Transform.Position = _groupBefore[i].Position + delta;
                }
                return;
            }

            if (_boxSelecting)
            {
                _boxCurrent = world;
                return;
            }

            if (_panning)
            {
                _editor.Camera.Pan(screen - _lastScreen);
                _lastScreen = screen;
            }
        }

        public void Up()
        {
            if (_movingGroup || _rotating || _scaling)
            {
                var after = _groupObjects.Select(EditorViewModel.Capture).ToArray();
                _editor.PushGroupTransform(_groupObjects, _groupBefore, after);
            }
            else if (_boxSelecting)
            {
                var box = BoxSelectWorld!.Value;
                var inside = Flatten(_editor.Scene.Objects)
                    .Where(o => o.IsVisible && Inside(box, o.Transform.WorldPosition))
                    .ToList();
                if (inside.Count > 0)
                    _editor.SetSelection(inside);
            }

            _movingGroup = _rotating = _scaling = _panning = _boxSelecting = false;
        }

        public void Wheel(Vector2 screen, int delta, int width, int height)
        {
            float factor = delta > 0 ? 1.1f : 1f / 1.1f;
            _editor.Camera.ZoomAt(screen, factor, width, height);
        }

        private void BeginGroupOp(Vector2 center)
        {
            _groupObjects = _editor.SelectedObjects.ToArray();
            _groupBefore = _groupObjects.Select(EditorViewModel.Capture).ToArray();
            _groupCenter = center;
        }

        /// <summary>Geometria do gizmo da seleção: centro, handle de rotação e cantos de escala.</summary>
        private (Vector2 Center, Vector2 Handle, Vector2[] Corners) Gizmo(IReadOnlyList<GameObject> selected, float zoom)
        {
            if (selected.Count == 1)
            {
                var obj = selected[0];
                return (obj.Transform.WorldPosition,
                        GizmoGeometry.RotationHandleWorld(obj, zoom),
                        GizmoGeometry.Corners(obj, SceneRenderer.GetVisualSize(obj)));
            }

            var bounds = GizmoGeometry.GroupBounds(selected);
            return (GizmoGeometry.GroupCenter(bounds),
                    GizmoGeometry.GroupRotationHandle(bounds, zoom),
                    GizmoGeometry.GroupCorners(bounds));
        }

        private GameObject? Pick(Vector2 world)
        {
            GameObject? found = null;
            foreach (var obj in Flatten(_editor.Scene.Objects))
            {
                if (!obj.IsVisible)
                    continue;

                var local = Vector2.Transform(world, Matrix.Invert(obj.Transform.WorldMatrix));
                var half = SceneRenderer.GetVisualSize(obj) / 2f;
                if (local.X >= -half.X && local.X <= half.X && local.Y >= -half.Y && local.Y <= half.Y)
                    found = obj;
            }
            return found;
        }

        private static bool Inside((Vector2 Min, Vector2 Max) box, Vector2 p)
            => p.X >= box.Min.X && p.X <= box.Max.X && p.Y >= box.Min.Y && p.Y <= box.Max.Y;

        private static float Angle(Vector2 from, Vector2 to)
            => (float)Math.Atan2(to.Y - from.Y, to.X - from.X);

        private static Vector2 Rotate(Vector2 v, float a)
        {
            float c = (float)Math.Cos(a), s = (float)Math.Sin(a);
            return new Vector2(v.X * c - v.Y * s, v.X * s + v.Y * c);
        }

        private static IEnumerable<GameObject> Flatten(IEnumerable<GameObject> objects)
        {
            foreach (var obj in objects)
            {
                yield return obj;
                foreach (var child in Flatten(obj.Children))
                    yield return child;
            }
        }
    }
}
