using System.Collections.Generic;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;

namespace DreamBit.Engine.Pooling
{
    /// <summary>
    /// Reaproveita instâncias de um objeto-modelo em vez de criar/destruir toda hora — bom
    /// para projéteis, inimigos e efeitos que aparecem em quantidade (menos alocação/GC).
    /// <see cref="Get"/> tira um objeto livre (ou clona um novo) e o ativa na cena;
    /// <see cref="Return"/> o remove da cena e o devolve à reserva. Uso típico em scripts.
    /// </summary>
    public sealed class ObjectPool
    {
        private readonly Scene _scene;
        private readonly GameObject _template;
        private readonly Stack<GameObject> _free = new();
        private readonly HashSet<GameObject> _active = new();

        /// <param name="scene">Cena onde os objetos entram/saem.</param>
        /// <param name="template">Objeto-modelo (não é adicionado; é clonado a cada criação).</param>
        /// <param name="prewarm">Quantos clones criar de antemão (evita alocação no primeiro uso).</param>
        public ObjectPool(Scene scene, GameObject template, int prewarm = 0)
        {
            _scene = scene;
            _template = template;
            for (int i = 0; i < prewarm; i++)
                _free.Push(CreateNew());
        }

        /// <summary>Objetos atualmente em uso (na cena).</summary>
        public int ActiveCount => _active.Count;

        /// <summary>Objetos prontos na reserva (fora da cena).</summary>
        public int FreeCount => _free.Count;

        /// <summary>Ativa um objeto: reutiliza da reserva ou clona um novo, adiciona à cena e
        /// reinicia seu estado (OnPlayStarted). O chamador posiciona o objeto depois.</summary>
        public GameObject Get()
        {
            var obj = _free.Count > 0 ? _free.Pop() : CreateNew();
            _active.Add(obj);
            _scene.Add(obj);
            obj.StartPlay(); // reseta timers/animações do objeto reaproveitado
            return obj;
        }

        /// <summary>Devolve um objeto à reserva (remove da cena). Ignora objetos que não são
        /// deste pool ou que já foram devolvidos.</summary>
        public bool Return(GameObject obj)
        {
            if (!_active.Remove(obj))
                return false;
            _scene.Remove(obj);
            _free.Push(obj);
            return true;
        }

        /// <summary>Devolve todos os objetos ativos à reserva.</summary>
        public void ReturnAll()
        {
            foreach (var obj in new List<GameObject>(_active))
                Return(obj);
        }

        private GameObject CreateNew() => SceneSerializer.CloneObject(_template);
    }
}
