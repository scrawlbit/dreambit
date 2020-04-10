using DreamBit.Extension.Management;
using Microsoft.Xna.Framework;
using Scrawlbit.Notification;
using System.ComponentModel;

namespace DreamBit.Extension.Module.Selection
{
    public interface ISelectionObject : INotifyPropertyChanged
    {
        string Name { get; }
        bool IsVisible { get; set; }
        ISelectionTransform Transform { get; }
        bool HasSelection { get; }
        bool HasMultipleSelection { get; }
        Rectangle Area { get; }
    }

    internal class SelectionObject : NotificationObject, ISelectionObject
    {
        private readonly IEditor _editor;
        private ISelectionMode _mode;
        private string _name;
        private bool _isVisible;
        private bool _hasSelection;
        private bool _hasMultipleSelection;
        private Rectangle _area;
        private Vector2 _areaOffset;

        public SelectionObject(IEditor editor)
        {
            _editor = editor;
            _editor.SelectedObjects.CollectionChanged += (s, e) => OnSelectedObjectsChanged();

            Transform = new SelectionTransform();

            OnSelectedObjectsChanged();
        }

        public string Name
        {
            get => _name;
            private set => Set(ref _name, value);
        }
        public bool IsVisible
        {
            get => _isVisible;
            set => Set(ref _isVisible, value);
        }
        public ISelectionTransform Transform { get; }
        public bool HasSelection
        {
            get => _hasSelection;
            private set => Set(ref _hasSelection, value);
        }
        public bool HasMultipleSelection
        {
            get => _hasMultipleSelection;
            private set => Set(ref _hasMultipleSelection, value);
        }
        public Rectangle Area
        {
            get => _area;
            private set
            {
                if (Equals(value, _area)) return;

                _area = value;
                _areaOffset = Transform.Position - Area.Location.ToVector2();

                OnPropertyChanged();
            }
        }

        private void OnSelectedObjectsChanged()
        {
            HasSelection = _editor.SelectedObjects.Count > 0;
            HasMultipleSelection = _editor.SelectedObjects.Count > 1;

            DetermineSelectionMode();
            Refresh();
        }

        private void DetermineSelectionMode()
        {
            _mode?.Dispose();

            if (HasMultipleSelection)
                _mode = new MultipleSelectionMode(_editor.SelectedObjects);
            else if (HasSelection)
                _mode = new SingleSelectionMode(_editor.SelectedObjects[0]);
            else
                _mode = new NoSelectionMode();

            _mode.NameChanged += UpdateName;
            _mode.AreaChanged += UpdateArea;
        }
        private void UpdateName()
        {
            Name = _mode.Name();
        }
        private void UpdateArea()
        {
            Area = _mode.Area();
            UpdateAreaPosition();
        }
        private void UpdateAreaPosition()
        {
            var location = Transform.Position - _areaOffset;

            Area = new Rectangle(location.ToPoint(), Area.Size);
        }

        private void Refresh()
        {
            IsVisible = _mode.IsVisible();
            Transform.Position = _mode.Position();
            Transform.Rotation = _mode.Rotation();
            Transform.Scale = _mode.Scale();
            UpdateName();
            UpdateArea();
        }
    }
}
