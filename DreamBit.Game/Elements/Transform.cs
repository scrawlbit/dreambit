using Microsoft.Xna.Framework;
using Scrawlbit.MonoGame.Helpers;
using Scrawlbit.Notification;
using System;

namespace DreamBit.Game.Elements
{
    public sealed class Transform : NotificationObject
    {
        private Matrix _transformations;
        private Transform _baseTransform;
        private bool _isTransformationsValid;
        private TransformChange _lastChange;

        // relative
        private Vector2 _relativePosition;
        private float _relativeRotation;
        private Vector2 _relativeScale;

        // real
        private Vector2 _realPosition;
        private float _realRotation;
        private Vector2 _realScale;

        public Transform()
        {
            _lastChange = TransformChange.Relative;
            RelativeScale = Vector2.One;
        }

        public event Action Invalidated;
        private bool IsTransformationsValid
        {
            get => _isTransformationsValid;
            set
            {
                if (value == _isTransformationsValid) return;

                _isTransformationsValid = value;
                Invalidated?.Invoke();
            }
        }
        internal Transform BaseTransform
        {
            get => _baseTransform;
            set
            {
                if (value == _baseTransform) return;

                if (_baseTransform != null)
                    _baseTransform.Invalidated -= OnBaseTransformInvalidated;

                ValidateTransformations();
                
                _baseTransform = value;
                _lastChange = TransformChange.Real;
                IsTransformationsValid = false;

                if (_baseTransform != null)
                    _baseTransform.Invalidated += OnBaseTransformInvalidated;
            }
        }

        // relative
        internal Vector2 RelativePosition
        {
            get
            {
                ValidateTransformations();
                return _relativePosition;
            }
            set
            {
                if (value != _relativePosition)
                {
                    InvalidateTransformations(TransformChange.Relative);
                    _relativePosition = value;
                }
            }
        }
        internal float RelativeRotation
        {
            get
            {
                ValidateTransformations();
                return _relativeRotation;
            }
            set
            {
                value = value.PositiveAngle();

                if (!value.EqualTo(_relativeRotation))
                {
                    InvalidateTransformations(TransformChange.Relative);
                    _relativeRotation = value;
                }
            }
        }
        internal Vector2 RelativeScale
        {
            get
            {
                ValidateTransformations();
                return _relativeScale;
            }
            set
            {
                value = value.MinimumScale();

                if (value != _relativeScale)
                {
                    InvalidateTransformations(TransformChange.Relative);
                    _relativeScale = value;
                }
            }
        }

        // real
        public Vector2 Position
        {
            get
            {
                ValidateTransformations();
                return _realPosition;
            }
            set
            {
                if (value != _realPosition)
                {
                    OnPropertyChanging(_realPosition, value);
                    InvalidateTransformations(TransformChange.Real);
                    _realPosition = value;
                    OnPropertyChanged();
                }
            }
        }
        public float Rotation
        {
            get
            {
                ValidateTransformations();
                return _realRotation;
            }
            set
            {
                value = value.PositiveAngle();

                if (!value.EqualTo(_realRotation))
                {
                    OnPropertyChanging(_realRotation, value);
                    InvalidateTransformations(TransformChange.Real);
                    _realRotation = value;
                    OnPropertyChanged();
                }
            }
        }
        public Vector2 Scale
        {
            get
            {
                ValidateTransformations();
                return _realScale;
            }
            set
            {
                value = value.MinimumScale();

                if (value != _realScale)
                {
                    OnPropertyChanging(_realScale, value);
                    InvalidateTransformations(TransformChange.Real);
                    _realScale = value;
                    OnPropertyChanged();
                }
            }
        }

        public Matrix Matrix
        {
            get
            {
                ValidateTransformations();
                return _transformations;
            }
        }

        private void OnBaseTransformInvalidated()
        {
            InvalidateTransformations(TransformChange.Relative);
        }
        private void InvalidateTransformations(TransformChange change)
        {
            IsTransformationsValid = false;

            if (_lastChange == TransformChange.Relative && change == TransformChange.Real) TransformValues();
            if (_lastChange == TransformChange.Real && change == TransformChange.Relative) TransformValues();

            _lastChange = change;
        }
        internal void ValidateTransformations()
        {
            if (IsTransformationsValid)
                return;

            BaseTransform?.ValidateTransformations();
            TransformValues();
            IsTransformationsValid = true;
        }

        private void TransformValues()
        {
            if (_lastChange == TransformChange.Real)
                TransformRealValuesToRelative();
            else
                TransformRelativeValuesToReal();
        }
        private void TransformRelativeValuesToReal()
        {
            var matrix = MatrixHelper.Create(_relativePosition, _relativeRotation, _relativeScale);
            var baseMatrix = BaseTransform?._transformations ?? Matrix.Identity;

            _transformations = matrix * baseMatrix;
            _transformations.Decompose(out _realPosition, out _realRotation, out _realScale);

            if (_realRotation < 0)
                _realRotation = MathHelper.TwoPi + _realRotation;
        }
        private void TransformRealValuesToRelative()
        {
            _transformations = MatrixHelper.Create(_realPosition, _realRotation, _realScale);

            var baseMatrix = BaseTransform?._transformations ?? Matrix.Identity;
            var difference = _transformations * baseMatrix.Invert();

            difference.Decompose(out _relativePosition, out _relativeRotation, out _relativeScale);

            if (_relativeRotation < 0)
                _relativeRotation = MathHelper.TwoPi + _relativeRotation;
        }
    }
}