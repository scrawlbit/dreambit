using System.Collections.Generic;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;

namespace DreamBit.Engine.Physics
{
    /// <summary>Coleta as caixas sólidas da cena (BoxColliders ativos e tiles sólidos) com o objeto dono de cada uma.</summary>
    public static class SolidQuery
    {
        /// <summary>Sólidos da cena, exceto os do objeto <paramref name="exclude"/>. Tiles têm dono null.</summary>
        public static List<SolidPhysics.Box> Collect(Scene? scene, GameObject? exclude, out List<GameObject?> owners)
        {
            var boxes = new List<SolidPhysics.Box>();
            owners = new List<GameObject?>();
            if (scene == null)
                return boxes;

            foreach (var obj in All(scene.Objects))
            {
                if (obj == exclude)
                    continue;
                foreach (var component in obj.Components)
                {
                    if (!component.Enabled)
                        continue;
                    if (component is BoxCollider collider)
                    {
                        var (min, max) = collider.WorldBounds();
                        boxes.Add(new SolidPhysics.Box(min, max));
                        owners.Add(obj);
                    }
                    else if (component is TilemapRenderer tilemap)
                    {
                        foreach (var (min, max) in tilemap.SolidBoxes())
                        {
                            boxes.Add(new SolidPhysics.Box(min, max));
                            owners.Add(null);
                        }
                    }
                }
            }
            return boxes;
        }

        private static IEnumerable<GameObject> All(IEnumerable<GameObject> objects)
        {
            foreach (var obj in objects)
            {
                yield return obj;
                foreach (var child in All(obj.Children))
                    yield return child;
            }
        }
    }
}
