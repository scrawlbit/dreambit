using DreamBit.Game.Elements;
using DreamBit.Game.Elements.Components;
using DreamBit.General.State;
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
        private readonly IStateManager _state;

        public MoveGameObjectCommand(IStateManager state)
        {
            _state = state;
        }

        public bool CanExecute(TreeViewDropEventArgs e)
        {
            return IsDataValid(e) && IsHierarchyValid(e) && !IsMovingCameraInsideObject(e);
        }
        public void Execute(TreeViewDropEventArgs e)
        {
            var initialObjects = e.Data.Cast<GameObject>().ToArray();
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

            _state.Execute(new MoveGameObjectsState(initialObjects, data, GetDescription(e)));
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
        private static bool IsMovingCameraInsideObject(TreeViewDropEventArgs e)
        {
            if (e.Target is GameObject)
            {
                var objects = e.Data.Cast<GameObject>();
                var components = objects.SelectMany(o => o.Components);
                var cameras = components.OfType<Camera>();

                return cameras.Any();
            }

            return false;
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
        private static string GetDescription(TreeViewDropEventArgs e)
        {
            string description = "{0} moved to {1}";
            string targetName = (e.Target as GameObject)?.Name;

            if (e.HasMultipleData)
                return $"Game objects moved to {targetName ?? "Scene"}";

            if (Equals(e.SingleSource.From, e.To))
            {
                if (e.DropType == DropType.Inside)
                    targetName = e.To.Cast<GameObject>().Last().Name;
                else if (e.DropType != DropType.InsideOnTop)
                    targetName = (e.OriginalTarget as GameObject)?.Name;

                if (e.DropType == DropType.Above)
                    description = "{0} moved above {1}";
                else if (e.DropType == DropType.Below || e.DropType == DropType.Inside)
                    description = "{0} moved below {1}";
            }

            description = description.FormatWith(((GameObject)e.SingleData).Name, targetName ?? "Scene");

            return description;
        }

        private static void Move(GameObject gameObject, MoveData from, MoveData to)
        {
            if (from.Collection == to.Collection)
            {
                from.Collection.Move(from.Index, to.Index);
                return;
            }

            from.Collection.Remove(gameObject);

            if (to.Index > to.Collection.Count)
                to.Collection.Add(gameObject);
            else
                to.Collection.Insert(to.Index, gameObject);

            if (to.Parent != null)
                to.Parent.IsExpanded = true;
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

        #endregion
        #region GameObjectMoveData

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
        #region MoveGameObjectState

        private class MoveGameObjectsState : IStateCommand
        {
            private GameObject[] _initialObjects;
            private readonly GameObjectMoveData[] _data;

            public MoveGameObjectsState(GameObject[] initialObjects, GameObjectMoveData[] data, string description)
            {
                _initialObjects = initialObjects;
                _data = data;

                Description = description;
            }

            public string Description { get; }

            public void Do()
            {
                SetIsMoving(true);
                _data.ForEach(d => Move(d.GameObject, d.From, d.To));
                SetIsMoving(false);
            }
            public void Undo()
            {
                SetIsMoving(true);
                _data.ForEach(d => Move(d.GameObject, d.To, d.From));
                SetIsMoving(false);
            }

            private void SetIsMoving(bool isMoving)
            {
                _initialObjects.ForEach(g => g.IsMoving = isMoving);
            }
        }

        #endregion
    }
}
