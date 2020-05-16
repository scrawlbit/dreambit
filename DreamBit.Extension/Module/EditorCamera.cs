using Microsoft.Xna.Framework;
using Scrawlbit.MonoGame.Helpers;
using Scrawlbit.Util.Helpers;
using System;

namespace DreamBit.Extension.Module
{
    public interface IEditorCamera
    {
        float MinZoom { get; }
        float MaxZoom { get; }
        Vector2 Size { get; set; }
        Vector2 Position { get; set; }
        float Zoom { get; set; }
        Matrix TransformMatrix { get; }

        void ZoomIn(float increment = 0);
        void ZoomOut(float increment = 0);

        Point ScreenToWorld(int x, int y);
        Vector2 ScreenToWorld(float x, float y);
        Vector2 ScreenToWorld(Vector2 screenPosition);
        Rectangle ScreenToWorld(Rectangle screenRectangle);

        Point WorldToScreen(int x, int y);
        Vector2 WorldToScreen(float x, float y);
        Vector2 WorldToScreen(Vector2 worldPosition);
        Rectangle WorldToScreen(Rectangle worldRectangle);
    }

    internal class EditorCamera : IEditorCamera
    {
        private Vector2 _size;
        private Vector2 _position;
        private float _zoom;

        public EditorCamera()
        {
            Zoom = 1;
        }

        public float MinZoom => 0.1f;
        public float MaxZoom => 2f;
        public Vector2 Size
        {
            get => _size;
            set
            {
                if (Variable.Set(ref _size, value))
                    UpdateTransformMatrix();
            }
        }
        public Vector2 Position
        {
            get => _position;
            set
            {
                if (Variable.Set(ref _position, value))
                    UpdateTransformMatrix();
            }
        }
        public float Zoom
        {
            get => _zoom;
            set
            {
                value = MathHelper.Clamp(value, MinZoom, MaxZoom);

                if (Variable.Set(ref _zoom, value.Clamp(MinZoom, MaxZoom)))
                    UpdateTransformMatrix();
            }
        }
        public Matrix TransformMatrix { get; private set; }
        private float Increment => 0.1f;

        private void UpdateTransformMatrix()
        {
            TransformMatrix =
                MatrixHelper.CreateScale(new Vector2(Zoom)) *
                MatrixHelper.CreateTranslation(-Position) *
                MatrixHelper.CreateTranslation(Size * .5f);
        }

        public void ZoomIn(float increment)
        {
            increment = Math.Abs(increment);
            increment = Math.Max(increment, Increment);

            Zoom += increment;
        }
        public void ZoomOut(float increment)
        {
            increment = Math.Abs(increment);
            increment = Math.Max(increment, Increment);

            Zoom -= increment;
        }

        public Point ScreenToWorld(int x, int y)
        {
            return ScreenToWorld(new Vector2(x, y)).ToPoint();
        }
        public Vector2 ScreenToWorld(float x, float y)
        {
            return ScreenToWorld(new Vector2(x, y));
        }
        public Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            return Vector2.Transform(screenPosition, Matrix.Invert(TransformMatrix));
        }
        public Rectangle ScreenToWorld(Rectangle screenRectangle)
        {
            var a = ScreenToWorld(screenRectangle.Left, screenRectangle.Top);
            var b = ScreenToWorld(screenRectangle.Right, screenRectangle.Bottom);
            var size = b - a;

            return new Rectangle(a, size);
        }

        public Point WorldToScreen(int x, int y)
        {
            return WorldToScreen(new Vector2(x, y)).ToPoint();
        }
        public Vector2 WorldToScreen(float x, float y)
        {
            return WorldToScreen(new Vector2(x, y));
        }
        public Vector2 WorldToScreen(Vector2 worldPosition)
        {
            return Vector2.Transform(worldPosition, TransformMatrix);
        }
        public Rectangle WorldToScreen(Rectangle worldRectangle)
        {
            var a = WorldToScreen(worldRectangle.Left, worldRectangle.Top);
            var b = WorldToScreen(worldRectangle.Right, worldRectangle.Bottom);
            var size = b - a;

            return new Rectangle(a, size);
        }
    }
}
