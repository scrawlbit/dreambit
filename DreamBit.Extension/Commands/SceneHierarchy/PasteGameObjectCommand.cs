using DreamBit.Extension.Helpers;
using DreamBit.Extension.Management;
using DreamBit.Game.Elements;
using DreamBit.Game.Elements.Components;
using DreamBit.Game.Serialization;
using DreamBit.General.State;
using Scrawlbit.Helpers;
using Scrawlbit.Presentation.Commands;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace DreamBit.Extension.Commands.SceneHierarchy
{
    public interface IPasteGameObjectCommand : ICommand
    {
        bool CanExecute();
        void Execute();
        void Execute(GameObject gameObject);
    }
    public sealed class PasteGameObjectCommand : BaseCommand, IPasteGameObjectCommand
    {
        private readonly IEditor _editor;
        private readonly IStateManager _state;
        private readonly IGameElementsParser _parser;

        public PasteGameObjectCommand(IEditor editor, IStateManager state, IGameElementsParser parser)
        {
            _editor = editor;
            _state = state;
            _parser = parser;

            _parser.DeserializeIds = false;
        }

        public bool CanExecute()
        {
            return _editor.OpenedScene != null;
        }
        public void Execute()
        {
            Paste(_editor.OpenedScene.Objects);
        }
        public void Execute(GameObject gameObject)
        {
            Paste(gameObject.Children, gameObject);

            gameObject.IsExpanded = true;
        }

        private void Paste(IGameObjectCollection collection, GameObject parent = null)
        {
            try
            {
                string json = Clipboard.GetText();
                GameObject gameObject = _parser.ToGameObject(json);

                if (parent != null)
                    RemoveCamera(gameObject);

                RemoveCameras(gameObject.Children);
                FixActiveCamera(gameObject);

                string parentName = parent?.Name ?? "Scene";
                string description = $"{gameObject.Name} added to {parentName}";
                IStateCommand command = collection.State().Add(gameObject, description);

                _state.Execute(command);

                // TODO _editor.SelectedObject = gameObject;
            }
            catch
            {
            }
        }

        private void RemoveCamera(GameObject gameObject)
        {
            Camera camera = gameObject.Components.Find<Camera>();

            if (camera != null)
                gameObject.Components.Remove(camera);
        }
        private void RemoveCameras(IGameObjectCollection gameObjects)
        {
            foreach (var gameObject in gameObjects)
            {
                RemoveCamera(gameObject);
                RemoveCameras(gameObject.Children);
            }
        }
        private void FixActiveCamera(GameObject gameObject)
        {
            Camera camera = gameObject.Components.Find<Camera>();
            
            if (camera?.IsActive == true && _editor.OpenedScene.GetCameras().Any(c => c.IsActive))
                camera.IsActive = false;
        }
    }
}