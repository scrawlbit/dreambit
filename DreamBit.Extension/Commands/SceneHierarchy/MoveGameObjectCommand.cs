using DreamBit.Game.Elements;
using Microsoft.Xna.Framework;
using Scrawlbit.Collections;
using Scrawlbit.Helpers;
using Scrawlbit.MonoGame.Helpers;
using Scrawlbit.Presentation.Commands;
using Scrawlbit.Presentation.DragAndDrop;
using System.Collections;
using System.Linq;
using System.Windows.Input;

namespace DreamBit.Extension.Commands.SceneHierarchy
{
    internal interface IMoveGameObjectCommand : ICommand
    {
        bool CanExecute(TreeViewDropEventArgs e);
        void Execute(TreeViewDropEventArgs e);
    }

    internal class MoveGameObjectCommand : BaseCommand, IMoveGameObjectCommand
    {
        public bool CanExecute(TreeViewDropEventArgs e)
        {
            return IsDataValid(e) && IsHierarchyValid(e);
        }
        public void Execute(TreeViewDropEventArgs e)
        {
            //var initialObjects = e.Data.Cast<GameObject>().ToArray();
            var gameObjects = GetGameObjectsToMove(e);
            var data = new GameObjectMoveData[gameObjects.Length];

            for (var i = 0; i < gameObjects.Length; i++)
            {
                var gameObject = gameObjects[i];
                var source = e.Sources[gameObject];
                var target = e.Target as GameObject;

                var from = new MoveData(gameObject.Parent, source.From, source.FromIndex);
                var to = new MoveData(target, e.To, e.ToIndex + i);

                data[i] = new GameObjectMoveData(gameObject, from, to);
            }

            data.ForEach(d => Move(d.GameObject, d.From, d.To));
        }

        private static bool IsDataValid(TreeViewDropEventArgs e)
        {
            return e.Data.All(d => d is GameObject);
        }
        private static bool IsHierarchyValid(TreeViewDropEventArgs e)
        {
            foreach (GameObject gameObject in e.Data)
            {
                var parent = e.Target as GameObject;
                while ((parent = parent?.Parent) != null)
                {
                    if (parent == gameObject) return false;
                }
            }

            return true;
        }
        private GameObject[] GetGameObjectsToMove(TreeViewDropEventArgs e)
        {
            return e.Data.Cast<GameObject>().Where(g =>
            {
                var parent = g;
                while ((parent = parent.Parent) != null)
                {
                    if (e.Data.Contains(parent)) return false;
                }

                return true;
            }).ToArray();
        }

        private void Move(GameObject gameObject, MoveData from, MoveData to)
        {
            if (from.Collection == to.Collection)
            {
                from.Collection.Move(from.Index, to.Index);
                return;
            }

            var a = gameObject.Transform.Matrix;
            var b = to.Parent?.Transform.Matrix ?? Matrix.Identity;
            var c = a * b.Invert();

            c.Decompose(out Vector2 position, out float rotation, out Vector2 scale);

            from.Collection.Remove(gameObject);

            if (to.Index > to.Collection.Count)
                to.Collection.Add(gameObject);
            else
                to.Collection.Insert(to.Index, gameObject);

            if (to.Parent != null)
                to.Parent.IsExpanded = true;

            gameObject.Transform.Position = position;
            gameObject.Transform.Rotation = rotation;
            gameObject.Transform.Scale = scale;
        }

        #region MoveData

        private class MoveData
        {
            public readonly GameObject Parent;
            public readonly IGameObjectCollection Collection;
            public readonly int Index;

            public MoveData(GameObject parent, IEnumerable collection, int index)
            {
                Parent = parent;
                Collection = (IGameObjectCollection)collection;
                Index = index;
            }
        }
        private class GameObjectMoveData
        {
            public readonly GameObject GameObject;
            public readonly MoveData From;
            public readonly MoveData To;

            public GameObjectMoveData(GameObject gameObject, MoveData from, MoveData to)
            {
                From = from;
                To = to;
                GameObject = gameObject;
            }
        }

        #endregion
    }
}
