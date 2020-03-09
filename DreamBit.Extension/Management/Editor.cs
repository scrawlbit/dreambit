using DreamBit.Game.Elements;
using DreamBit.Game.Files;
using Scrawlbit.Collections;
using Scrawlbit.Notification;
using System.ComponentModel;

namespace DreamBit.Extension.Management
{
    public interface IEditor : INotifyPropertyChanged
    {
        SceneFile OpenedSceneFile { get; set; }
        Scene OpenedScene { get; set; }
        GameObject SelectedObject { get; set; }
        IObservableCollection<GameObject> SelectedObjects { get; }
    }

    public class Editor : NotificationObject, IEditor
    {
        private SceneFile _openedSceneFile;
        private Scene _openedScene;
        private GameObject _selectedObject;

        public Editor()
        {
            SelectedObjects = new ExtendedObservableCollection<GameObject>();
        }

        public SceneFile OpenedSceneFile
        {
            get => _openedSceneFile;
            set => Set(ref _openedSceneFile, value);
        }
        public Scene OpenedScene
        {
            get => _openedScene;
            set => Set(ref _openedScene, value);
        }
        public GameObject SelectedObject
        {
            get => _selectedObject;
            set => Set(ref _selectedObject, value);
        }
        public IObservableCollection<GameObject> SelectedObjects { get; }
    }
}
