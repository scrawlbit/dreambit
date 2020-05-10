using DreamBit.Extension.Module;
using DreamBit.Game.Elements;
using DreamBit.Game.Files;
using DreamBit.General.State;
using Scrawlbit.Collections;
using Scrawlbit.Notification;
using System;
using System.ComponentModel;

namespace DreamBit.Extension.Management
{
    public interface IEditor : INotifyPropertyChanged
    {
        ISceneEditorCamera Camera { get; }
        SceneFile OpenedSceneFile { get; set; }
        Scene OpenedScene { get; }
        GameObject SelectedObject { get; set; }
        IObservableCollection<GameObject> SelectedObjects { get; }
        ISelectionObject Selection { get; }
    }

    internal class Editor : NotificationObject, IEditor
    {
        private readonly IStateManager _state;
        private readonly Lazy<ISelectionObject> _selection;
        private SceneFile _openedSceneFile;
        private Scene _openedScene;
        private GameObject _selectedObject;

        public Editor(ISceneEditorCamera camera, IStateManager state, Lazy<ISelectionObject> selection)
        {
            _state = state;
            _selection = selection;

            Camera = camera;
            SelectedObjects = new ExtendedObservableCollection<GameObject>();
        }

        public ISceneEditorCamera Camera { get; }
        public SceneFile OpenedSceneFile
        {
            get => _openedSceneFile;
            set
            {
                if (Set(ref _openedSceneFile, value))
                {
                    value?.Load();
                    OpenedScene = value?.Scene;
                }
            }
        }
        public Scene OpenedScene
        {
            get => _openedScene;
            private set
            {
                if (Set(ref _openedScene, value))
                {
                    _state.Reset();
                    SelectedObject = null;
                    SelectedObjects.Clear();
                }
            }
        }
        public GameObject SelectedObject
        {
            get => _selectedObject;
            set => Set(ref _selectedObject, value);
        }
        public IObservableCollection<GameObject> SelectedObjects { get; }
        public ISelectionObject Selection => _selection.Value;
    }
}
