using System;
using DreamBit.Engine.Notification;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Elements
{
    /// <summary>
    /// Transform hierárquico 2D (posição/rotação/escala locais + composição com o pai):
    /// a matriz de mundo é a matriz local multiplicada pela matriz de mundo do pai.
    /// </summary>
    public sealed class Transform : NotificationObject
    {
        private Vector2 _position;
        private float _rotation;
        private Vector2 _scale = Vector2.One;
        private Transform? _parent;

        public event Action? Changed;

        internal Transform? Parent
        {
            get => _parent;
            set
            {
                if (_parent == value)
                    return;

                if (_parent != null)
                    _parent.Changed -= OnParentChanged;

                _parent = value;

                if (_parent != null)
                    _parent.Changed += OnParentChanged;

                RaiseChanged();
            }
        }

        /// <summary>Posição local (relativa ao pai).</summary>
        public Vector2 Position
        {
            get => _position;
            set { if (Set(ref _position, value)) RaiseChanged(); }
        }

        /// <summary>Rotação local em radianos.</summary>
        public float Rotation
        {
            get => _rotation;
            set { if (Set(ref _rotation, NormalizeAngle(value))) RaiseChanged(); }
        }

        /// <summary>Escala local.</summary>
        public Vector2 Scale
        {
            get => _scale;
            set { if (Set(ref _scale, value)) RaiseChanged(); }
        }

        /// <summary>Matriz local (sem o pai).</summary>
        public Matrix LocalMatrix =>
            Matrix.CreateScale(_scale.X, _scale.Y, 1f) *
            Matrix.CreateRotationZ(_rotation) *
            Matrix.CreateTranslation(_position.X, _position.Y, 0f);

        /// <summary>Matriz de mundo (composta com a hierarquia).</summary>
        public Matrix WorldMatrix =>
            _parent == null ? LocalMatrix : LocalMatrix * _parent.WorldMatrix;

        /// <summary>Posição absoluta em coordenadas de mundo.</summary>
        public Vector2 WorldPosition => Vector2.Transform(Vector2.Zero, WorldMatrix);

        /// <summary>Rotação absoluta (soma da cadeia de pais).</summary>
        public float WorldRotation => _rotation + (_parent?.WorldRotation ?? 0f);

        /// <summary>Escala absoluta (produto da cadeia de pais).</summary>
        public Vector2 WorldScale => _parent == null ? _scale : _scale * _parent.WorldScale;

        /// <summary>Define o transform local para que o objeto fique nesta pose de mundo,
        /// respeitando o pai atual. Usado ao reparentar sem mover o objeto na tela.</summary>
        public void SetWorld(Vector2 worldPosition, float worldRotation, Vector2 worldScale)
        {
            if (_parent == null)
            {
                Position = worldPosition;
                Rotation = worldRotation;
                Scale = worldScale;
                return;
            }

            Position = Vector2.Transform(worldPosition, Matrix.Invert(_parent.WorldMatrix));
            Rotation = worldRotation - _parent.WorldRotation;
            var ps = _parent.WorldScale;
            Scale = new Vector2(
                ps.X != 0f ? worldScale.X / ps.X : worldScale.X,
                ps.Y != 0f ? worldScale.Y / ps.Y : worldScale.Y);
        }

        private void OnParentChanged() => RaiseChanged();

        private void RaiseChanged()
        {
            Changed?.Invoke();
            OnPropertyChanged(nameof(WorldMatrix));
            OnPropertyChanged(nameof(WorldPosition));
        }

        private static float NormalizeAngle(float angle)
        {
            angle %= MathHelper.TwoPi;
            if (angle < 0)
                angle += MathHelper.TwoPi;
            return angle;
        }
    }
}
