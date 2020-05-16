using DreamBit.Extension.Management;
using DreamBit.Extension.Module.TransformHandlers;
using DreamBit.Game.Drawing;
using DreamBit.Game.Elements;
using Microsoft.Xna.Framework;
using Scrawlbit.Helpers;
using Scrawlbit.MonoGame.Helpers;
using ScrawlBit.MonoGame.Interop.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace DreamBit.Extension.Module.Tools
{
    internal interface ISelectionTool : IEditorTool { }
    internal class SelectionTool : EditorTool, ISelectionTool
    {
        private readonly IEditor _editor;
        private readonly ITransformHandler[] _handlers;
        private bool _isSelecting;
        private Point _initialPosition;
        private Rectangle _selectionArea;
        private bool _addSelection;

        public SelectionTool(
            IEditor editor,
            IMoveHandler moveHandler,
            IRotateHandler rotateHandler,
            IScaleHandler scaleHandler)
        {
            _editor = editor;
            _handlers = new ITransformHandler[]
            {
                moveHandler,
                rotateHandler,
                scaleHandler
            };
        }

        public override string Icon => "cursor";
        public override Key ShortcutKey => Key.V;

        public override void OnKeyDown(KeyEventArgs e)
        {
            if (!e.IsRepeat && e.Key.In(Key.LeftCtrl, Key.RightCtrl, Key.LeftShift, Key.RightShift))
                _addSelection = true;
        }
        public override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key.In(Key.LeftCtrl, Key.RightCtrl, Key.LeftShift, Key.RightShift))
            {
                if (_addSelection && _isSelecting)
                    _editor.CleanSelection();

                _addSelection = false;
            }
        }

        public override void OnMouseDown(GameMouseButtonEventArgs args)
        {
            if (IsMouseOverHandler(args.Position, out ITransformHandler handler))
            {

            }
            else if (args.ChangedButton == MouseButton.Left)
            {
                _isSelecting = true;
                _initialPosition = args.Position.ToPoint();
                _selectionArea = new Rectangle(_initialPosition, Point.Zero);

                if (!_addSelection)
                    _editor.CleanSelection();
            }

            args.Handled = true;
        }
        public override void OnMouseMove(GameMouseEventArgs args)
        {
            if (_isSelecting)
            {
                Point location = _initialPosition;
                Point size = args.Position.ToPoint() - location;

                _selectionArea = new Rectangle(location, size).Positive();
            }

            args.Handled = true;
        }
        public override void OnMouseUp(GameMouseButtonEventArgs args)
        {
            if (_isSelecting && args.ChangedButton == MouseButton.Left)
            {
                Rectangle area = _editor.Camera.ScreenToWorld(_selectionArea);
                var gameObjects = GetObjectsInArea(_editor.OpenedScene.Objects, area);

                if (!area.HasSize())
                    gameObjects = gameObjects.Take(1);

                _editor.SelectObjects(false, gameObjects.ToArray());
                _selectionArea = Rectangle.Empty;
                _isSelecting = false;
            }

            args.Handled = true;
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

            for (int i = 0; i < _handlers.Length; i++)
                _handlers[i].Draw(drawer);
        }

        private bool IsMouseOverHandler(Vector2 position, out ITransformHandler handler)
        {
            for (int i = 0; i < _handlers.Length; i++)
            {
                if ((handler = _handlers[i]).IsMouseOver(position))
                    return true;
            }

            handler = null;
            return false;
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
