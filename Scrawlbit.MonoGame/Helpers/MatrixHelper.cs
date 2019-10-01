using System;
using Microsoft.Xna.Framework;

namespace Scrawlbit.MonoGame.Helpers
{
    public static class MatrixHelper
    {
        public static Matrix CreateScale(Vector2 scale)
        {
            return Matrix.CreateScale(scale.X, scale.Y, 1);
        }
        public static Matrix CreateRotation(float rotation)
        {
            return Matrix.CreateRotationZ(rotation);
        }
        public static Matrix CreateTranslation(Vector2 position)
        {
            return Matrix.CreateTranslation(position.X, position.Y, 0);
        }

        public static Matrix Create(Vector2 position, float rotation, Vector2 scale)
        {
            return CreateScale(scale) *
                   CreateRotation(rotation) *
                   CreateTranslation(position);
        }

        public static void Decompose(this Matrix transform, out Vector2 position, out float rotation, out Vector2 scale)
        {
            Vector3 position3;
            Vector3 scale3;
            Quaternion rotationQ;
            transform.Decompose(out scale3, out rotationQ, out position3);

            var direction = Vector2.Transform(Vector2.UnitX, rotationQ);

            position = new Vector2(position3.X, position3.Y);
            rotation = (float)Math.Atan2(direction.Y, direction.X);
            scale = new Vector2(scale3.X, scale3.Y);
        }

        public static Vector2 DecomposePosition(this Matrix transform)
        {
            Vector2 position;
            float rotation;
            Vector2 scale;
            transform.Decompose(out position, out rotation, out scale);

            return position;
        }
        public static Vector2 DecomposeScale(this Matrix transform)
        {
            Vector2 position;
            float rotation;
            Vector2 scale;
            transform.Decompose(out position, out rotation, out scale);

            return scale;
        }
        public static float DecomposeRotation(this Matrix transform)
        {
            Vector2 position;
            float rotation;
            Vector2 scale;
            transform.Decompose(out position, out rotation, out scale);

            return rotation;
        }

        public static Matrix Invert(this Matrix matrix)
        {
            return Matrix.Invert(matrix);
        }
    }
}