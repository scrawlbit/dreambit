using Microsoft.Xna.Framework;
using Scrawlbit.MonoGame.Helpers;
using Scrawlbit.Notification;
using System;

namespace DreamBit.Game.Elements
{
    public sealed class Transform : ITransformationValues
    {
        private readonly TransformationValues _relative;
        private readonly TransformationValues _real;
        private Transform _baseTransform;
        private bool _isTransformationsValid;
        private TransformChange _lastChange;

        public Transform()
        {
            _relative = new TransformationValues(this, TransformChange.Relative);
            _real = new TransformationValues(this, TransformChange.Real);

            _lastChange = TransformChange.Relative;
            ((ITransformationValues)_relative).Scale = Vector2.One;
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

        private ITransformationValues Real => _real;
        public ITransformationValues Relative => _relative;
        public Vector2 Position
        {
            get => Real.Position;
            set => Real.Position = value;
        }
        public float Rotation
        {
            get => Real.Rotation;
            set => Real.Rotation = value;
        }
        public Vector2 Scale
        {
            get => Real.Scale;
            set => Real.Scale = value;
        }
        public Matrix Matrix
        {
            get => Real.Matrix;
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
                TransformValues(_real, _relative);
            else
                TransformValues(_relative, _real);
        }
        private void TransformValues(TransformationValues from, TransformationValues to)
        {
            from.Matrix = MatrixHelper.Create(from.Position, from.Rotation, from.Scale);

            var baseMatrix = BaseTransform?._real.Matrix ?? Matrix.Identity;

            if (to == _relative)
                baseMatrix = baseMatrix.Invert();

            to.Matrix = from.Matrix * baseMatrix;
            to.Matrix.Decompose(out to.Position, out to.Rotation, out to.Scale);

            if (to.Rotation < 0)
                to.Rotation = MathHelper.TwoPi + to.Rotation;
        }

        private class TransformationValues : ITransformationValues
        {
            private readonly Transform _transform;
            private readonly TransformChange _changesType;

            public TransformationValues(Transform transform, TransformChange changesType)
            {
                _transform = transform;
                _changesType = changesType;
            }

            public Matrix Matrix;
            public Vector2 Position;
            public float Rotation;
            public Vector2 Scale;

            Vector2 ITransformationValues.Position
            {
                get
                {
                    _transform.ValidateTransformations();
                    return Position;
                }
                set
                {
                    if (value != Position)
                    {
                        _transform.InvalidateTransformations(_changesType);
                        Position = value;
                    }
                }
            }
            float ITransformationValues.Rotation
            {
                get
                {
                    _transform.ValidateTransformations();
                    return Rotation;
                }
                set
                {
                    value = value.PositiveAngle();

                    if (!value.EqualTo(Rotation))
                    {
                        _transform.InvalidateTransformations(_changesType);
                        Rotation = value;
                    }
                }
            }
            Vector2 ITransformationValues.Scale
            {
                get
                {
                    _transform.ValidateTransformations();
                    return Scale;
                }
                set
                {
                    value = value.MinimumScale();

                    if (value != Scale)
                    {
                        _transform.InvalidateTransformations(_changesType);
                        Scale = value;
                    }
                }
            }
            Matrix ITransformationValues.Matrix
            {
                get
                {
                    _transform.ValidateTransformations();
                    return Matrix;
                }
            }
        }
    }
}