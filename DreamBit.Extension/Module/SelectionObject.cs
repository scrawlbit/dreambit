using DreamBit.Extension.Management;
using DreamBit.Game.Elements;
using DreamBit.Game.Helpers;
using DreamBit.General.State;
using Microsoft.Xna.Framework;
using Scrawlbit.MonoGame.Helpers;
using Scrawlbit.Notification;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DreamBit.Extension.Module
{
    public interface ISelectionObject : INotifyPropertyChanged
    {
        bool HasSelection { get; }
        bool HasOneSelection { get; }
        bool HasMultipleSelection { get; }
        string Name { get; }
        bool? IsVisible { get; set; }
        float X { get; set; }
        float Y { get; set; }
        float Rotation { get; set; }
        float ScaleX { get; set; }
        float ScaleY { get; set; }

        Rectangle Area();
        void ValidateChanges();
    }

    internal class SelectionObject : NotificationObject, ISelectionObject
    {
        private readonly IEditor _editor;
        private readonly IStateManager _state;
        private readonly IDictionary<GameObject, SelectionData> _data;
        private GameObject[] _gameObjects;
        private Matrix _initialMatrix;
        private bool _hasSelection;
        private bool _hasOneSelection;
        private bool _hasMultipleSelection;
        private string _name;
        private bool? _isVisible;
        private Vector2 _position;
        private float _rotation;
        private Vector2 _scale;
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
                if (Set(ref _position.X, value.EnsurePrecision()))
                    ApplyChanges();
            }
        }
        public float Y
        {
            get => _position.Y;
            set
            {
                if (Set(ref _position.Y, value.EnsurePrecision()))
                    ApplyChanges();
            }
        }
        public float Rotation
        {
            get => _rotation;
            set
            {
                value = value.EnsurePrecision();
                value = value.PositiveAngle();

                if (Set(ref _rotation, value))
                    ApplyChanges();
            }
        }
        public float ScaleX
        {
            get => _scale.X;
            set
            {
                value = value.EnsurePrecision();
                value = value.MinimumScale();

                if (Set(ref _scale.X, value))
                    ApplyChanges();
            }
        }
        public float ScaleY
        {
            get => _scale.Y;
            set
            {
                if (Set(ref _scale.Y, value.EnsurePrecision()))
                    ApplyChanges();
            }
        }
        private Vector2 Position
        {
            get => _position;
        }
        private Vector2 Scale
        {
            get => _scale;
        }

        public Rectangle Area()
        {
            Rectangle area = _gameObjects.TotalArea();
            Vector2 offset = Position - area.Location.ToVector2();
            Vector2 location = Position - offset;

            return new Rectangle(location.ToPoint(), area.Size);
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
                    ValidatePositionChange(gameObject, data);
                    ValidateRotationChange(gameObject, data);
                    ValidateScaleChange(gameObject, data);
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

            Vector2 position = DeterminePosition();
            Vector2 scale = DetermineScale();

            Name = DetermineName();
            IsVisible = DetermineIsVisible();
            X = position.X;
            Y = position.Y;
            Rotation = DetermineRotation();
            ScaleX = scale.X;
            ScaleY = scale.Y;

            CopyData(true);

            _applyChanges = true;
        }
        private void ApplyChanges()
        {
            if (!_applyChanges)
                return;

            Matrix offset = _initialMatrix.Invert();
            Matrix changes = offset * DetermineMatrix();

            foreach (var gameObject in _gameObjects)
            {
                SelectionData data = _data[gameObject];
                Matrix transform = data.Matrix * changes;

                transform.Decompose(out Vector2 position, out float rotation, out Vector2 scale);

                gameObject.IsVisible = IsVisible ?? data.IsVisible;
                gameObject.Transform.Position = position.EnsurePrecision();
                gameObject.Transform.Rotation = rotation.EnsurePrecision();
                gameObject.Transform.Scale = scale.EnsurePrecision();
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
        private Vector2 DeterminePosition()
        {
            if (HasMultipleSelection)
            {
                Rectangle area = _gameObjects.TotalArea();
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
        private Matrix DetermineMatrix()
        {
            return MatrixHelper.Create(Position, Rotation, Scale);
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
                data.Matrix = gameObject.Transform.Matrix;
            }

            _initialMatrix = DetermineMatrix();
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
        private void ValidatePositionChange(GameObject gameObject, SelectionData data)
        {
            if (gameObject.Transform.Position == data.Position)
                return;

            string value = gameObject.Transform.Position.Text();
            string description = $"{gameObject.Name} position changed to {value}";
            IStateCommand command = gameObject.Transform.State().SetProperty(t => t.Position, data.Position, gameObject.Transform.Position, description);

            _state.Add(command);
        }
        private void ValidateRotationChange(GameObject gameObject, SelectionData data)
        {
            if (gameObject.Transform.Rotation == data.Rotation)
                return;

            string value = gameObject.Transform.Rotation.Text();
            string description = $"{gameObject.Name} rotation changed to {value}";
            IStateCommand command = gameObject.Transform.State().SetProperty(t => t.Rotation, data.Rotation, gameObject.Transform.Rotation, description);

            _state.Add(command);
        }
        private void ValidateScaleChange(GameObject gameObject, SelectionData data)
        {
            if (gameObject.Transform.Scale == data.Scale)
                return;

            string value = gameObject.Transform.Scale.Text();
            string description = $"{gameObject.Name} scale changed to {value}";
            IStateCommand command = gameObject.Transform.State().SetProperty(t => t.Scale, data.Scale, gameObject.Transform.Scale, description);

            _state.Add(command);
        }
    }
}
