using System.Collections.Generic;
using DreamBit.Engine.Rendering;

namespace DreamBit.Engine.Elements
{
    /// <summary>
    /// Consultas sobre o que está dentro do quadro da câmera neste frame (ex.: quantos
    /// inimigos aparecem na tela agora). Usa o estado de câmera publicado em <see cref="Screen"/>
    /// pelo host (Player/preview), então funciona em scripts, componentes e testes.
    /// </summary>
    public static class CameraVision
    {
        /// <summary>True se o objeto está visível e dentro do quadro da câmera.</summary>
        public static bool IsOnScreen(GameObject obj, float margin = 0f)
            => obj != null && obj.IsVisible && Screen.IsOnScreen(obj.Transform.WorldPosition, margin);

        /// <summary>Objetos da cena com a tag informada (ou todos, se tag vazia) que estão na tela.</summary>
        public static IEnumerable<GameObject> OnScreen(Scene scene, string? tag = null, float margin = 0f)
        {
            if (scene == null)
                yield break;
            foreach (var obj in Walk(scene.Objects))
            {
                if (!IsOnScreen(obj, margin))
                    continue;
                if (string.IsNullOrEmpty(tag) || obj.HasTag(tag))
                    yield return obj;
            }
        }

        /// <summary>Quantidade de objetos com a tag informada visíveis na câmera agora.</summary>
        public static int CountOnScreen(Scene scene, string tag, float margin = 0f)
        {
            int total = 0;
            if (scene == null)
                return 0;
            foreach (var obj in Walk(scene.Objects))
                if (obj.HasTag(tag) && IsOnScreen(obj, margin))
                    total++;
            return total;
        }

        private static IEnumerable<GameObject> Walk(IEnumerable<GameObject> objects)
        {
            foreach (var obj in objects)
            {
                yield return obj;
                foreach (var child in Walk(obj.Children))
                    yield return child;
            }
        }
    }
}
