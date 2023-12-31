﻿using DreamBit.Extension.Management;
using DreamBit.Game.Elements;
using DreamBit.Game.Helpers;
using DreamBit.General.State;
using Microsoft.Xna.Framework;
using Scrawlbit.MonoGame.Helpers;
using Scrawlbit.Notification;
using Scrawlbit.Notification.Notificator;
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
        Vector2 Position { get; set; }
        float Rotation { get; set; }
        Vector2 Scale { get; set; }

        Rectangle Area();
        void ValidateChanges();
    }

    internal class SelectionObject : NotificationObject, ISelectionObject
    {
        private readonly IEditor _editor;
        private readonly IStateManager _state;
        private readonly IDictionary<GameObject, SelectionData> _data;
        private GameObject[] _gameObjects;
        private Notificator<GameObject> _notifications;
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
        public Vector2 Position
        {
            get => _position;
            set
            {
                if (Set(ref _position, value.EnsurePrecision()))
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
        public Vector2 Scale
        {
            get => _scale;
            set
            {
                value = value.EnsurePrecision();
                value = value.MinimumScale();

                if (Set(ref _scale, value))
                    ApplyChanges();
            }
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
            List<IStateCommand> states = GetChanges().ToList();

            if (!states.Any())
                return;

            states.Insert(0, new StateCommand("Update selection", undo: UpdateTransformations));
            states.Add(new StateCommand("Update selection", @do: UpdateTransformations));

            using (_state.Scope("Selection updated"))
                states.ForEach(_state.Add);

            CopyData();
        }

        private void Update()
        {
            _applyChanges = false;

            _notifications?.Dispose();
            _notifications = null;
            _gameObjects = _editor.SelectedObjects.ToArray();

            HasSelection = _gameObjects.Length > 0;
            HasOneSelection = _gameObjects.Length == 1;
            HasMultipleSelection = _gameObjects.Length > 1;

            Name = DetermineName();
            IsVisible = DetermineIsVisible();
            Position = DeterminePosition();
            Rotation = DetermineRotation();
            Scale = DetermineScale();

            CopyData(true);
            TrackNameChanges();

            _applyChanges = true;
        }
        private void UpdateTransformations()
        {
            _applyChanges = false;

            IsVisible = DetermineIsVisible();
            Position = DeterminePosition();
            Rotation = DetermineRotation();
            Scale = DetermineScale();

            CopyData();

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
        private void TrackNameChanges()
        {
            if (!HasOneSelection)
                return;

            GameObject gameObject = _gameObjects[0];

            _notifications = _gameObjects[0].Notify()
                .On(g => g.Name).Changed(() => Name = gameObject.Name);
        }

        private IEnumerable<IStateCommand> GetChanges()
        {
            foreach (var item in _data)
            {
                GameObject gameObject = item.Key;
                SelectionData data = item.Value;

                if (gameObject.IsVisible != data.IsVisible)
                    yield return CreateIsVisibleState(gameObject, data);

                if (gameObject.Transform.Position != data.Position)
                    yield return CreatePositionState(gameObject, data);

                if (gameObject.Transform.Rotation != data.Rotation)
                    yield return CreateRotationState(gameObject, data);

                if (gameObject.Transform.Scale != data.Scale)
                    yield return CreateScaleState(gameObject, data);
            }
        }
        private static IStateCommand CreateIsVisibleState(GameObject gameObject, SelectionData data)
        {
            string value = gameObject.IsVisible ? "visible" : "invisible";
            string description = $"{gameObject.Name} set to {value}";
            IStateCommand command = gameObject.State().SetProperty(g => g.IsVisible, data.IsVisible, gameObject.IsVisible, description);

            return command;
        }
        private static IStateCommand CreatePositionState(GameObject gameObject, SelectionData data)
        {
            string value = gameObject.Transform.Position.Text();
            string description = $"{gameObject.Name} position changed to {value}";
            IStateCommand command = gameObject.Transform.State().SetProperty(t => t.Position, data.Position, gameObject.Transform.Position, description);

            return command;
        }
        private static IStateCommand CreateRotationState(GameObject gameObject, SelectionData data)
        {
            string value = gameObject.Transform.Rotation.Text();
            string description = $"{gameObject.Name} rotation changed to {value}";
            IStateCommand command = gameObject.Transform.State().SetProperty(t => t.Rotation, data.Rotation, gameObject.Transform.Rotation, description);

            return command;
        }
        private static IStateCommand CreateScaleState(GameObject gameObject, SelectionData data)
        {
            string value = gameObject.Transform.Scale.Text();
            string description = $"{gameObject.Name} scale changed to {value}";
            IStateCommand command = gameObject.Transform.State().SetProperty(t => t.Scale, data.Scale, gameObject.Transform.Scale, description);

            return command;
        }
    }
}
