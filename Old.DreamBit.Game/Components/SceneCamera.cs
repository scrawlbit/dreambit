using System;
using DreamBit.Game.Helpers;
using Microsoft.Xna.Framework;
using Scrawlbit.MonoGame.Helpers;

namespace DreamBit.Game.Components
{
    internal class SceneCamera : ISceneCamera
    {
        private Vector2 _size;
        private Vector2 _position;
        private float _rotation;
        private Vector2 _zoom;
        private bool _isTransformationsValid;
        private Matrix _transformMatrix;

        public SceneCamera()
        {
            _zoom = Vector2.One;
        }

        public Vector2 Size
        {
            get => _size;
            set
            {
                if (value.SetTo(ref _size))
                    InvalidateTransformations();
            }
        }
        public Vector2 Position
        {
            get => _position;
            set
            {
                if (value.SetTo(ref _position))
                    InvalidateTransformations();
            }
        }
        public float Rotation
        {
            get => _rotation;
            set
            {
                value = value.PositiveAngle();

                if (value.SetTo(ref _rotation))
                    InvalidateTransformations();
            }
        }
        public Vector2 Zoom
        {
            get => _zoom;
            set
            {
                value = value.MinimumScale();

                if (value.SetTo(ref _zoom))
                    InvalidateTransformations();
            }
        }
        public Matrix TransformMatrix
        {
            get
            {
                ValidateTransformations();
                return _transformMatrix;
            }
        }

        private void InvalidateTransformations()
        {
            _isTransformationsValid = false;
        }
        internal void ValidateTransformations()
        {
            if (!_isTransformationsValid)
            {
                _isTransformationsValid = true;
                _transformMatrix = MatrixHelper.Create(-_position, Rotation, Zoom) *
                                   MatrixHelper.CreateTranslation(Size * .5f);
            }
        }

        public void ZoomIn(float value)
        {
            Zoom += new Vector2(Math.Abs(value));
        }
        public void ZoomOut(float value)
        {
            Zoom -= new Vector2(Math.Abs(value));
        }
    }
}