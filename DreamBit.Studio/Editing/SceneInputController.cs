using System.Collections.Generic;
using System.Linq;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using DreamBit.Studio.ViewModels;
using Microsoft.Xna.Framework;

namespace DreamBit.Studio.Editing
{
    /// <summary>
    /// Traduz o input do mouse no canvas em operações de edição: seleção por clique
    /// (picking tela→mundo), arraste do objeto selecionado, pan e zoom da câmera.
    /// </summary>
    public sealed class SceneInputController
    {
        private readonly EditorViewModel _editor;

        private bool _dragging;
        private Vector2 _dragOffset;
        private GameObject? _dragObject;
        private Vector2 _dragStartPosition;
        private GameObject? _rotateObject;
        private float _rotateStart;
        private bool _panning;
        private Vector2 _lastScreen;

        public SceneInputController(EditorViewModel editor) => _editor = editor;

        public void PrimaryDown(Vector2 screen, int width, int height)
        {
            var world = _editor.Camera.ScreenToWorld(screen, width, height);

            // Gizmo de rotação do objeto já selecionado tem prioridade sobre o pick.
            var selected = _editor.SelectedObject;
            if (selected != null)
            {
                var handle = GizmoGeometry.RotationHandleWorld(selected, _editor.Camera.Zoom);
                if (Vector2.Distance(world, handle) <= GizmoGeometry.GrabScreenRadius / _editor.Camera.Zoom)
                {
                    _rotateObject = selected;
                    _rotateStart = selected.Transform.Rotation;
                    return;
                }
            }

            var hit = Pick(world);
            _editor.SelectedObject = hit;

            if (hit != null)
            {
                _dragging = true;
                _dragObject = hit;
                _dragStartPosition = hit.Transform.Position;
                _dragOffset = hit.Transform.Position - ToLocalParent(hit, world);
            }
        }

        public void MiddleDown(Vector2 screen)
        {
            _panning = true;
            _lastScreen = screen;
        }

        public void Move(Vector2 screen, int width, int height)
        {
            if (_rotateObject != null)
            {
                var world = _editor.Camera.ScreenToWorld(screen, width, height);
                _rotateObject.Transform.Rotation = GizmoGeometry.RotationTowards(_rotateObject, world);
                return;
            }

            if (_panning)
            {
                _editor.Camera.Pan(screen - _lastScreen);
                _lastScreen = screen;
                return;
            }

            if (_dragging && _editor.SelectedObject != null)
            {
                var world = _editor.Camera.ScreenToWorld(screen, width, height);
                var position = ToLocalParent(_editor.SelectedObject, world) + _dragOffset;

                if (_editor.SnapToGrid && _editor.GridStep > 0)
                {
                    float step = _editor.GridStep;
                    position = new Vector2(
                        (float)System.Math.Round(position.X / step) * step,
                        (float)System.Math.Round(position.Y / step) * step);
                }

                _editor.SelectedObject.Transform.Position = position;
            }
        }

        public void Up()
        {
            if (_rotateObject != null)
            {
                _editor.PushRotation(_rotateObject, _rotateStart, _rotateObject.Transform.Rotation);
                _rotateObject = null;
            }

            if (_dragObject != null)
            {
                _editor.PushMove(_dragObject, _dragStartPosition, _dragObject.Transform.Position);
                _dragObject = null;
            }

            _dragging = false;
            _panning = false;
        }

        public void Wheel(Vector2 screen, int delta, int width, int height)
        {
            float factor = delta > 0 ? 1.1f : 1f / 1.1f;
            _editor.Camera.ZoomAt(screen, factor, width, height);
        }

        /// <summary>Objeto no topo (último desenhado) sob o ponto de mundo, ou null.</summary>
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
                    found = obj; // continua para pegar o mais "por cima"
            }

            return found;
        }

        /// <summary>Converte um ponto de mundo para o espaço do pai do objeto (onde vive Position).</summary>
        private static Vector2 ToLocalParent(GameObject obj, Vector2 world)
        {
            var parent = obj.Parent;
            return parent == null ? world : Vector2.Transform(world, Matrix.Invert(parent.Transform.WorldMatrix));
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
