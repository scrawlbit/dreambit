using System;
using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Rendering
{
    /// <summary>
    /// Geometria dos gizmos de transformação, compartilhada entre o renderizador
    /// (desenho) e o controlador de input (hit-test), para ficarem sempre coerentes.
    /// As distâncias são em pixels de tela (divididas pelo zoom para virarem mundo).
    /// </summary>
    public static class GizmoGeometry
    {
        public const float RotationScreenRadius = 55f;
        public const float GrabScreenRadius = 12f;

        /// <summary>Rotação de mundo do objeto (extraída da matriz de mundo).</summary>
        public static float WorldRotation(GameObject obj)
        {
            var m = obj.Transform.WorldMatrix;
            return (float)Math.Atan2(m.M12, m.M11);
        }

        /// <summary>Direção "para cima" do objeto para uma rotação de mundo.</summary>
        public static Vector2 UpDirection(float worldRotation)
            => new((float)Math.Sin(worldRotation), -(float)Math.Cos(worldRotation));

        /// <summary>Posição de mundo do handle de rotação (acima do centro do objeto).</summary>
        public static Vector2 RotationHandleWorld(GameObject obj, float zoom)
        {
            var center = obj.Transform.WorldPosition;
            var up = UpDirection(WorldRotation(obj));
            return center + up * (RotationScreenRadius / zoom);
        }

        /// <summary>Os 4 cantos do objeto em coordenadas de mundo (para os handles de escala).</summary>
        public static Vector2[] Corners(GameObject obj, Vector2 size)
        {
            var w = obj.Transform.WorldMatrix;
            var h = size / 2f;
            return new[]
            {
                Vector2.Transform(new Vector2(-h.X, -h.Y), w),
                Vector2.Transform(new Vector2(h.X, -h.Y), w),
                Vector2.Transform(new Vector2(h.X, h.Y), w),
                Vector2.Transform(new Vector2(-h.X, h.Y), w)
            };
        }

        /// <summary>
        /// Rotação LOCAL que faz o "para cima" do objeto apontar para <paramref name="worldTarget"/>.
        /// </summary>
        public static float RotationTowards(GameObject obj, Vector2 worldTarget)
        {
            var center = obj.Transform.WorldPosition;
            var d = worldTarget - center;
            float worldAngle = (float)Math.Atan2(d.X, -d.Y);

            float parentWorld = obj.Parent != null ? WorldRotation(obj.Parent) : 0f;
            return worldAngle - parentWorld;
        }
    }
}
