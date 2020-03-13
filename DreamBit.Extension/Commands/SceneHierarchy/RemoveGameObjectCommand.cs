using DreamBit.Extension.Helpers;
using DreamBit.Extension.Management;
using DreamBit.Game.Elements;
using DreamBit.General.State;
using Scrawlbit.Presentation.Commands;
using System.Linq;
using System.Windows.Input;

namespace DreamBit.Extension.Commands.SceneHierarchy
{
    internal interface IRemoveGameObjectCommand : ICommand
    {
        void Execute(GameObject gameObject);
    }

    internal sealed class RemoveGameObjectCommand : BaseCommand, IRemoveGameObjectCommand
    {
        private readonly IEditor _editor;
        private readonly IStateManager _state;

        public RemoveGameObjectCommand(IEditor editor, IStateManager state)
        {
            _editor = editor;
            _state = state;
        }

        public void Execute(GameObject reference)
        {
            using (_state.Scope("Game objects removed from hierarchy"))
            {
                foreach (var gameObject in GetObjectsToRemove(reference))
                {
                    string parentName = gameObject.Parent?.Name ?? "Scene";
                    string description = $"{gameObject.Name} removed from {parentName}";
                    
                    IGameObjectCollection collection = gameObject.Parent?.Children ?? _editor.OpenedScene.Objects;
                    IStateCommand command = collection.State().Remove(gameObject, description);

                    _state.Execute(command);
                }
            }
        }

        private GameObject[] GetObjectsToRemove(GameObject gameObject)
        {
            GameObject[] gameObjects = _editor.SelectedObjects.Contains(gameObject)
                ? _editor.SelectedObjects.ToArray()
                : new[] { gameObject };

            return gameObjects.Where(g =>
            {
                GameObject parent = g;

                while ((parent = parent.Parent) != null)
                {
                    if (gameObjects.Contains(parent))
                        return false;
                }

                return true;
            }).ToArray();
        }
    }
}