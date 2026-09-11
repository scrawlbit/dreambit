using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Notification;
using DreamBit.Engine.Rendering;
using DreamBit.Studio.Mvvm;
using Microsoft.Xna.Framework;

namespace DreamBit.Studio.ViewModels
{
    /// <summary>
    /// Estado do editor: a cena, a câmera, o objeto selecionado e os comandos da toolbar.
    /// </summary>
    public sealed class EditorViewModel : NotificationObject
    {
        private GameObject? _selectedObject;
        private int _counter;
        private bool _isPlaying;

        public EditorViewModel()
        {
            Scene = new Scene { Name = "Cena de Exemplo" };
            Camera = new Camera2D();
            Inspector = new InspectorViewModel();

            AddObjectCommand = new RelayCommand(() => AddObject());
            DeleteObjectCommand = new RelayCommand(DeleteSelected, () => SelectedObject != null);

            SeedSampleScene();
        }

        public Scene Scene { get; }
        public Camera2D Camera { get; }
        public InspectorViewModel Inspector { get; }

        public RelayCommand AddObjectCommand { get; }
        public RelayCommand DeleteObjectCommand { get; }

        public GameObject? SelectedObject
        {
            get => _selectedObject;
            set
            {
                if (_selectedObject == value)
                    return;

                if (_selectedObject != null)
                    _selectedObject.IsSelected = false;

                _selectedObject = value;

                if (_selectedObject != null)
                    _selectedObject.IsSelected = true;

                Inspector.Target = _selectedObject;
                OnPropertyChanged();
                DeleteObjectCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsPlaying
        {
            get => _isPlaying;
            set => Set(ref _isPlaying, value);
        }

        public GameObject AddObject()
        {
            var obj = new GameObject($"GameObject {++_counter}");
            obj.AddComponent(new SpriteRenderer
            {
                Size = new Vector2(96, 64),
                Color = new Color(70 + _counter * 25 % 150, 130, 200)
            });
            Scene.Add(obj);
            SelectedObject = obj;
            return obj;
        }

        public void DeleteSelected()
        {
            if (SelectedObject == null)
                return;

            var toRemove = SelectedObject;
            SelectedObject = null;
            Scene.Remove(toRemove);
        }

        private void SeedSampleScene()
        {
            var a = AddObject();
            a.Transform.Position = new Vector2(-140, -40);

            var b = AddObject();
            b.Transform.Position = new Vector2(120, 60);
            b.Transform.Rotation = 0.3f;

            SelectedObject = a;
        }
    }
}
