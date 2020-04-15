using DreamBit.Extension.Management;
using DreamBit.Game.Elements;
using DreamBit.General.State;
using Microsoft.Xna.Framework;
using Scrawlbit.Helpers;
using Scrawlbit.Notification;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DreamBit.Extension.Module
{
    public interface ISelectionObject : INotifyPropertyChanged
    {
        bool HasSelection { get; }
        bool HasMultipleSelection { get; }
        string Name { get; }
        bool? IsVisible { get; set; }
        float X { get; set; }
        float Y { get; set; }
        float Rotation { get; set; }
        float ScaleX { get; set; }
        float ScaleY { get; set; }
        Rectangle Area { get; }

        void ValidateChanges();
    }

    internal class SelectionObject : NotificationObject, ISelectionObject
    {
        private readonly IEditor _editor;
        private readonly IStateManager _state;
        private readonly IDictionary<GameObject, SelectionData> _data;
        private GameObject[] _gameObjects;
        private bool _hasSelection;
        private bool _hasOneSelection;
        private bool _hasMultipleSelection;
        private string _name;
        private bool? _isVisible;
        private Vector2 _position;
        private float _rotation;
        private Vector2 _scale;
        private Rectangle _area;
        private bool _applyChanges;

        public SelectionObject(IEditor editor, IStateManager state)
        {
            _editor = editor;
            _state = state;
            _data = new Dictionary<GameObject, SelectionData>();
            
            _editor.SelectedObjects.CollectionChanged += (s, e) => Update();

            Update();
        }

        public bool HasSelection
        {
            get => _hasSelection;
            private set => Set(ref _hasSelection, value);
        }
        public bool HasOneSelection
        {
            get => _hasOneSelection;
            private set => Set(ref _hasOneSelection, value);
        }
        public bool HasMultipleSelection
        {
            get => _hasMultipleSelection;
            private set => Set(ref _hasMultipleSelection, value);
        }
        public string Name
        {
            get => _name;
            private set => Set(ref _name, value);
        }
        public bool? IsVisible
        {
            get => _isVisible;
            set
            {
                if (HasOneSelection)
                    value = value ?? false;

                if (Set(ref _isVisible, value))
                    ApplyChanges();
            }
        }
        public float X
        {
            get => _position.X;
            set
            {
                if (Set(ref _position.X, EnsurePrecision(value)))
                    ApplyChanges();
            }
        }
        public float Y
        {
            get => _position.Y;
            set
            {
                if (Set(ref _position.Y, EnsurePrecision(value)))
                    ApplyChanges();
            }
        }
        public float Rotation
        {
            get => _rotation;
            set
            {
                if (Set(ref _rotation, EnsurePrecision(value)))
                    ApplyChanges();
            }
        }
        public float ScaleX
        {
            get => _scale.X;
            set
            {
                if (Set(ref _scale.X, EnsurePrecision(value)))
                    ApplyChanges();
            }
        }
        public float ScaleY
        {
            get => _scale.Y;
            set
            {
                if (Set(ref _scale.Y, EnsurePrecision(value)))
                    ApplyChanges();
            }
        }
        public Rectangle Area
        {
            get => _area;
            private set => Set(ref _area, value);
        }

        public void ValidateChanges()
        {
            using (_state.Scope("Selection updated"))
            {
                foreach (var item in _data)
                {
                    GameObject gameObject = item.Key;
                    SelectionData data = item.Value;

                    ValidateIsVisibleChange(gameObject, data);
                }
            }

            CopyData();
        }

        private void Update()
        {
            _applyChanges = false;

            _gameObjects = _editor.SelectedObjects.ToArray();

            HasSelection = _gameObjects.Length > 0;
            HasOneSelection = _gameObjects.Length == 1;
            HasMultipleSelection = _gameObjects.Length > 1;

            Rectangle area = _gameObjects.TotalArea();
            Vector2 position = DeterminePosition(area);
            Vector2 scale = DetermineScale();

            Name = DetermineName();
            IsVisible = DetermineIsVisible();
            X = position.X;
            Y = position.Y;
            Rotation = DetermineRotation();
            ScaleX = scale.X;
            ScaleY = scale.Y;
            Area = DetermineArea(area);

            CopyData(true);

            _applyChanges = true;
        }
        private void ApplyChanges()
        {
            if (!_applyChanges)
                return;

            foreach (var gameObject in _gameObjects)
            {
                gameObject.IsVisible = IsVisible ?? _data[gameObject].IsVisible;
            }
        }

        private bool? DetermineIsVisible()
        {
            bool[] values = _gameObjects.Select(g => g.IsVisible).Distinct().ToArray();

            if (values.Length == 1)
                return values[0];

            return null;
        }
        private string DetermineName()
        {
            if (HasMultipleSelection) return "Selection";
            if (HasOneSelection) return _gameObjects[0].Name;

            return null;
        }
        private Vector2 DeterminePosition(Rectangle area)
        {
            if (HasMultipleSelection)
            {
                float x = MathHelper.Lerp(area.Left, area.Right, 0.5f);
                float y = MathHelper.Lerp(area.Top, area.Bottom, 0.5f);

                return new Vector2(x, y);
            }

            if (HasOneSelection)
                return _gameObjects[0].Transform.Position;

            return Vector2.Zero;
        }
        private float DetermineRotation()
        {
            if (HasOneSelection)
                return _gameObjects[0].Transform.Rotation;

            return 0;
        }
        private Vector2 DetermineScale()
        {
            if (HasOneSelection)
                return _gameObjects[0].Transform.Scale;

            return Vector2.One;
        }
        private Rectangle DetermineArea(Rectangle area)
        {
            Vector2 offset = _position - area.Location.ToVector2();
            Vector2 location = _position - offset;

            return new Rectangle(location.ToPoint(), area.Size);
        }
        private void CopyData(bool reset = false)
        {
            if (reset)
            {
                _data.Clear();

                foreach (var gameObject in _gameObjects)
                    _data[gameObject] = new SelectionData();
            }

            foreach (var item in _data)
            {
                GameObject gameObject = item.Key;
                SelectionData data = item.Value;

                data.IsVisible = gameObject.IsVisible;
                data.Position = gameObject.Transform.Position;
                data.Rotation = gameObject.Transform.Rotation;
                data.Scale = gameObject.Transform.Scale;
            }
        }

        private void ValidateIsVisibleChange(GameObject gameObject, SelectionData data)
        {
            if (gameObject.IsVisible == data.IsVisible)
                return;

            string value = gameObject.IsVisible ? "visible" : "invisible";
            string description = $"{gameObject.Name} set to {value}";
            IStateCommand command = gameObject.State().SetProperty(g => g.IsVisible, data.IsVisible, gameObject.IsVisible, description);

            _state.Add(command);
        }

        private static float EnsurePrecision(float value)
        {
            return (float)Math.Round(value, 3);
        }
    }
}
