using DreamBit.Extension.Management;
using DreamBit.Game.Drawing;
using DreamBit.Game.Elements;
using Microsoft.Xna.Framework;
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
        private bool _isSelecting;
        private Point _initialPosition;
        private Rectangle _selectionArea;
        private readonly IEditor _editor;

        public SelectionTool(IEditor editor)
        {
            _editor = editor;
        }

        public override string Icon => "cursor";
        public override Key ShortcutKey => Key.V;

        public override void OnMouseDown(GameMouseButtonEventArgs args)
        {
            if (args.ChangedButton == MouseButton.Left)
            {
                _isSelecting = true;
                _initialPosition = args.Position.ToPoint();
                _selectionArea = new Rectangle(_initialPosition, Point.Zero);
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

                _editor.SelectObjects(gameObjects.ToArray());
                _selectionArea = Rectangle.Empty;
                _isSelecting = false;
            }

            args.Handled = true;
        }

        public override void Draw(IContentDrawer drawer)
        {
            if (_selectionArea.HasSize())
            {
                drawer.Draw(_selectionArea, Color.Blue * 0.1f);
                drawer.DrawRectangle(_selectionArea, Color.Blue);
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
