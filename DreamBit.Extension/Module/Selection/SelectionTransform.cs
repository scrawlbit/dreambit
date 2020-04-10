using Microsoft.Xna.Framework;
using Scrawlbit.Notification;
using System;
using System.ComponentModel;

namespace DreamBit.Extension.Module.Selection
{
    public interface ISelectionTransform : INotifyPropertyChanged
    {
        float X { get; set; }
        float Y { get; set; }
        Vector2 Position { get; set; }
        float Rotation { get; set; }
        float ScaleX { get; set; }
        float ScaleY { get; set; }
        Vector2 Scale { get; set; }
    }

    internal class SelectionTransform : NotificationObject, ISelectionTransform
    {
        private Vector2 _position;
        private float _rotation;
        private Vector2 _scale;

        public float X
        {
            get => _position.X;
            set
            {
                if (Set(ref _position.X, value))
                    OnPropertyChanged(nameof(Position));
            }
        }
        public float Y
        {
            get => _position.Y;
            set
            {
                if (Set(ref _position.Y, value))
                    OnPropertyChanged(nameof(Position));
            }
        }
        public Vector2 Position
        {
            get => _position;
            set
            {
                if (Set(ref _position, value))
                {
                    OnPropertyChanged(nameof(X));
                    OnPropertyChanged(nameof(Y));
                }
            }
        }
        public float Rotation
        {
            get => _rotation;
            set => Set(ref _rotation, EnsurePrecision(value));
        }
        public float ScaleX
        {
            get => _scale.X;
            set
            {
                if (Set(ref _scale.X, value))
                    OnPropertyChanged(nameof(Scale));
            }
        }
        public float ScaleY
        {
            get => _scale.Y;
            set
            {
                if (Set(ref _scale.Y, value))
                    OnPropertyChanged(nameof(Scale));
            }
        }
        public Vector2 Scale
        {
            get => _scale;
            set
            {
                if (Set(ref _scale, value))
                {
                    OnPropertyChanged(nameof(ScaleX));
                    OnPropertyChanged(nameof(ScaleY));
                }
            }
        }

        public static float EnsurePrecision(float value)
        {
            return (float)Math.Round(value, 3);
        }
        public static Vector2 EnsurePrecision(Vector2 value)
        {
            value.X = EnsurePrecision(value.X);
            value.Y = EnsurePrecision(value.Y);

            return value;
        }
    }
}
