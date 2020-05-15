using DreamBit.Extension.Module;
using DreamBit.Game.Elements;
using DreamBit.Game.Files;
using DreamBit.General.State;
using DreamBit.Project;
using Scrawlbit.Collections;
using Scrawlbit.Notification;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace DreamBit.Extension.Management
{
    public interface IEditor : INotifyPropertyChanged
    {
        SceneFile OpenedSceneFile { get; set; }
        Scene OpenedScene { get; }
        GameObject SelectedObject { get; }
        IReadOnlyObservableCollection<GameObject> SelectedObjects { get; }
        IEditorCamera Camera { get; }
        IEditorToolBox ToolBox { get; }
        ISelectionObject Selection { get; }

        void SelectObjects(params GameObject[] objects);
    }

    internal class Editor : NotificationObject, IEditor
    {
        private readonly IStateManager _state;
        private readonly Lazy<IEditorToolBox> _toolBox;
        private readonly Lazy<ISelectionObject> _selection;
        private readonly IObservableCollection<GameObject> _selectedObjects;
        private SceneFile _openedSceneFile;
        private Scene _openedScene;
        private GameObject _selectedObject;

        public Editor(
            IProject project,
            IStateManager state,
            IEditorCamera camera,
            Lazy<IEditorToolBox> toolBox,
            Lazy<ISelectionObject> selection)
        {
            _state = state;
            _toolBox = toolBox;
            _selection = selection;
            _selectedObjects = new ExtendedObservableCollection<GameObject>();
            _selectedObjects.CollectionChanged += OnSelectedObjectsChanged;

            project.Notify().On(p => p.Loaded).Changed(OnProjectLoadedChanged);

            Camera = camera;
        }

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
                    _selectedObjects.Clear();
                }
            }
        }
        public GameObject SelectedObject
        {
            get => _selectedObject;
            private set => Set(ref _selectedObject, value);
        }
        public IReadOnlyObservableCollection<GameObject> SelectedObjects => _selectedObjects;
        public IEditorCamera Camera { get; }
        public IEditorToolBox ToolBox => _toolBox.Value;
        public ISelectionObject Selection => _selection.Value;

        public void SelectObjects(params GameObject[] objects)
        {
            _selectedObjects.Clear();
            _selectedObjects.AddRange(objects);
        }

        private void OnSelectedObjectsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SelectedObject = SelectedObjects.FirstOrDefault();
        }
        private void OnProjectLoadedChanged(bool loaded)
        {
            if (!loaded)
                OpenedSceneFile = null;
        }
    }
}
