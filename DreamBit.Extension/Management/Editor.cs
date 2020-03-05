using DreamBit.Game.Elements;
using Scrawlbit.Collections;
using Scrawlbit.Notification;
using System.ComponentModel;

namespace DreamBit.Extension.Management
{
    public interface IEditor : INotifyPropertyChanged
    {
        Scene OpenedScene { get; set; }
        GameObject SelectedObject { get; set; }
        IObservableCollection<GameObject> SelectedObjects { get; }
    }

    public class Editor : NotificationObject, IEditor
    {
        private Scene _openedScene;
        private GameObject _selectedObject;

        public Editor()
        {
            SelectedObjects = new ExtendedObservableCollection<GameObject>();
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
