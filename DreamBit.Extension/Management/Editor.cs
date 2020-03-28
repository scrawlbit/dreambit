using DreamBit.Game.Elements;
using DreamBit.Game.Files;
using DreamBit.General.State;
using Scrawlbit.Collections;
using Scrawlbit.Notification;
using System.ComponentModel;

namespace DreamBit.Extension.Management
{
    public interface IEditor : INotifyPropertyChanged
    {
        SceneFile OpenedSceneFile { get; set; }
        Scene OpenedScene { get; }
        GameObject SelectedObject { get; set; }
        IObservableCollection<GameObject> SelectedObjects { get; }
    }

    internal class Editor : NotificationObject, IEditor
    {
        private readonly IStateManager _state;
        private SceneFile _openedSceneFile;
        private Scene _openedScene;
        private GameObject _selectedObject;

        public Editor(IStateManager state)
        {
            _state = state;

            SelectedObjects = new ExtendedObservableCollection<GameObject>();
        }

        public SceneFile OpenedSceneFile
        {
            get => _openedSceneFile;
            set
            {
                if (Set(ref _openedSceneFile, value))
                    OpenedScene = value?.Scene;
            }
        }
        public Scene OpenedScene
        {
            get => _openedScene;
            private set
            {
                if (Set(ref _openedScene, value))
                    _state.Reset();
            }
        }
        public GameObject SelectedObject
        {
            get => _selectedObject;
            set => Set(ref _selectedObject, value);
        }
        public IObservableCollection<GameObject> SelectedObjects { get; }
    }
}
