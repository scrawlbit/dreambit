using System.Collections.ObjectModel;
using DreamBit.Engine.Notification;
using DreamBit.Engine.Rendering;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Elements
{
    /// <summary>
    /// Uma cena editável: coleção de objetos raiz. Equivalente ao Scene de DreamBit.Game.
    /// </summary>
    public sealed class Scene : NotificationObject
    {
        private string _name = "Nova Cena";
        private readonly ObservableCollection<GameObject> _objects = new();
        private readonly ObservableCollection<Ledge> _ledges = new();

        public Scene()
        {
            Objects = new ReadOnlyObservableCollection<GameObject>(_objects);
            Ledges = new ReadOnlyObservableCollection<Ledge>(_ledges);
        }

        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        /// <summary>Objetos raiz da cena (cada um pode ter filhos).</summary>
        public ReadOnlyObservableCollection<GameObject> Objects { get; }

        /// <summary>Ledges (bordas caminháveis) do mapa.</summary>
        public ReadOnlyObservableCollection<Ledge> Ledges { get; }

        public Ledge AddLedge(Ledge ledge)
        {
            _ledges.Add(ledge);
            return ledge;
        }

        public bool RemoveLedge(Ledge ledge) => _ledges.Remove(ledge);

        public GameObject Add(GameObject gameObject)
        {
            _objects.Add(gameObject);
            return gameObject;
        }

        public bool Remove(GameObject gameObject)
        {
            gameObject.Parent?.RemoveChild(gameObject);
            return _objects.Remove(gameObject);
        }

        public void Update(GameTime gameTime)
        {
            foreach (var gameObject in _objects)
                gameObject.Update(gameTime);
        }

        public void Draw(ISceneDrawing drawing)
        {
            foreach (var gameObject in _objects)
                gameObject.Draw(drawing);
        }
    }
}
