using DreamBit.Extension.Management;
using DreamBit.Game.Drawing;
using DreamBit.Game.Elements;
using Microsoft.Xna.Framework;
using Scrawlbit.Helpers;
using Scrawlbit.MonoGame.Helpers;
using ScrawlBit.MonoGame.Interop.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace DreamBit.Extension.Module.Handlers
{
    internal interface ISelectHandler : IEditorHandler { }
    internal class SelectHandler : EditorHandler, ISelectHandler
    {
        private readonly IEditor _editor;
        private Point _initialPosition;
        private Rectangle _selectionArea;
        private bool _addSelection;

        public SelectHandler(IEditor editor)
        {
            _editor = editor;
        }

        public override void OnKeyDown(KeyEventArgs e)
        {
            if (!e.IsRepeat && e.Key.In(Key.LeftCtrl, Key.RightCtrl, Key.LeftShift, Key.RightShift))
                _addSelection = true;
        }
        public override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key.In(Key.LeftCtrl, Key.RightCtrl, Key.LeftShift, Key.RightShift))
            {
                if (_addSelection && IsHandling)
                    _editor.CleanSelection();

                _addSelection = false;
            }
        }

        public override void OnMouseDown(GameMouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _initialPosition = e.Position.ToPoint();
                _selectionArea = new Rectangle(_initialPosition, Point.Zero);

                if (!_addSelection)
                    _editor.CleanSelection();

                IsHandling = true;
            }
        }
        public override void OnMouseMove(GameMouseEventArgs e)
        {
            if (IsHandling)
            {
                Point location = _initialPosition;
                Point size = e.Position.ToPoint() - location;

                _selectionArea = new Rectangle(location, size).Positive();
            }
        }
        public override void OnMouseUp(GameMouseButtonEventArgs e)
        {
            if (IsHandling && e.ChangedButton == MouseButton.Left)
            {
                Rectangle area = _editor.Camera.ScreenToWorld(_selectionArea);
                var gameObjects = GetObjectsInArea(_editor.OpenedScene.Objects, area);

                if (!area.HasSize())
                    gameObjects = gameObjects.Take(1);

                _editor.SelectObjects(false, gameObjects.ToArray());
                _selectionArea = Rectangle.Empty;

                IsHandling = false;
            }

            e.Handled = true;
        }

        public override void Draw(IContentDrawer drawer)
        {
            if (_selectionArea.HasSize())
            {
                drawer.FillRectangle(_selectionArea, Color.Blue * 0.1f);
                drawer.DrawRectangle(_selectionArea, Color.Blue);
            }

            if (_editor.Selection.HasSelection)
            {
                Rectangle selectionArea = _editor.Selection.Area();

                if (selectionArea.HasSize())
                {
                    selectionArea = _editor.Camera.WorldToScreen(selectionArea);
                    drawer.DrawRectangle(selectionArea, Color.White);
                }
            }
        }

        private IEnumerable<GameObject> GetObjectsInArea(IGameObjectCollection gameObjects, Rectangle area)
        {
            foreach (var gameObject in gameObjects.Reverse())
            {
                foreach (var child in GetObjectsInArea(gameObject.Children, area))
                    yield return child;

                if (area.Intersects(gameObject.Area()))
                    yield return gameObject;
            }
        }
    }
}
