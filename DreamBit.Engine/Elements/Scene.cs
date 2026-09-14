using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DreamBit.Engine.Notification;
using DreamBit.Engine.Rendering;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Elements
{
    /// <summary>
    /// Uma cena editável: coleção de objetos raiz.
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

        /// <summary>Esvazia a cena (objetos e ledges).</summary>
        public void Clear()
        {
            foreach (var obj in _objects.ToArray())
                obj.SetScene(null);
            _objects.Clear();
            _ledges.Clear();
        }

        /// <summary>Substitui o conteúdo desta cena pelo de outra (usado no snapshot de play).</summary>
        public void CopyFrom(Scene other)
        {
            Clear();
            Name = other.Name;
            foreach (var obj in other.Objects.ToArray())
                Add(obj);
            foreach (var ledge in other.Ledges.ToArray())
                AddLedge(ledge);
        }

        public GameObject Add(GameObject gameObject)
        {
            _objects.Add(gameObject);
            gameObject.SetScene(this);
            return gameObject;
        }

        public bool Remove(GameObject gameObject)
        {
            gameObject.Parent?.RemoveChild(gameObject);
            bool removed = _objects.Remove(gameObject);
            if (removed)
                gameObject.SetScene(null);
            return removed;
        }

        /// <summary>Índice de um objeto raiz (-1 se não for raiz).</summary>
        public int IndexOf(GameObject gameObject) => _objects.IndexOf(gameObject);

        /// <summary>Reordena um objeto raiz (muda a ordem de exibição entre irmãos).</summary>
        public void MoveObject(GameObject gameObject, int newIndex)
        {
            int old = _objects.IndexOf(gameObject);
            if (old < 0)
                return;
            newIndex = System.Math.Clamp(newIndex, 0, _objects.Count - 1);
            if (old != newIndex)
                _objects.Move(old, newIndex);
        }

        /// <summary>Insere um objeto raiz numa posição específica (tira do pai, se tiver).</summary>
        public GameObject Insert(GameObject gameObject, int index)
        {
            gameObject.Parent?.RemoveChild(gameObject);
            index = System.Math.Clamp(index, 0, _objects.Count);
            _objects.Insert(index, gameObject);
            gameObject.SetScene(this);
            return gameObject;
        }

        /// <summary>Mundo de física 2D (corpos rígidos) da cena, criado no início do play.</summary>
        private Physics2D.PhysicsWorld? _physics;
        public Physics2D.PhysicsWorld Physics => _physics ??= new Physics2D.PhysicsWorld(new Vector2(0f, 980f));

        /// <summary>Avisa todos os componentes que o play mode começou.</summary>
        public void StartPlay()
        {
            Timing.Scheduler.Clear(); // callbacks agendados não vazam entre execuções
            _physics = null;          // mundo de física novo a cada play (os corpos re-registram)
            foreach (var gameObject in _objects)
                gameObject.StartPlay();
        }

        // ---- barramento de mensagens (eventos de jogo) ----
        private readonly System.Collections.Generic.Queue<Messaging.GameMessage> _messages = new();

        /// <summary>Disparado quando qualquer mensagem é despachada (para logs/observadores).</summary>
        public event System.Action<Messaging.GameMessage>? MessageSent;

        /// <summary>Enfileira uma mensagem nomeada; será despachada no próximo Update.</summary>
        public void Send(string name, GameObject? sender = null)
        {
            if (!string.IsNullOrEmpty(name))
                _messages.Enqueue(new Messaging.GameMessage(name, sender));
        }

        // ---- transição de fase ----

        /// <summary>Arquivo de cena solicitado para carregar (transição de fase), ou null.
        /// O host (Player) verifica após o Update e troca de cena.</summary>
        public string? PendingSceneLoad { get; private set; }

        /// <summary>Solicita carregar outra fase (arquivo .dbscene, relativo à pasta atual).</summary>
        public void RequestSceneLoad(string sceneFile)
        {
            if (!string.IsNullOrWhiteSpace(sceneFile))
                PendingSceneLoad = sceneFile;
        }

        public void ClearPendingSceneLoad() => PendingSceneLoad = null;

        // Remoção adiada: componentes pedem destruição durante o Update; aplicamos ao final,
        // fora da iteração (evita mutar a coleção enquanto percorre).
        private readonly System.Collections.Generic.List<GameObject> _pendingDestroy = new();

        /// <summary>Agenda a remoção de um objeto ao final do frame (seguro durante o Update).</summary>
        public void Destroy(GameObject gameObject)
        {
            if (gameObject != null && !_pendingDestroy.Contains(gameObject))
                _pendingDestroy.Add(gameObject);
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Timing.Scheduler.Tick(dt);
            Audio.AudioMixer.Tick(dt); // recupera ducking da música/efeitos

            foreach (var gameObject in _objects.ToArray())
                gameObject.Update(gameTime);

            _physics?.Step(dt); // avança a física e sincroniza os Transforms dos corpos

            DispatchMessages();

            if (_pendingDestroy.Count > 0)
            {
                foreach (var obj in _pendingDestroy)
                    Remove(obj);
                _pendingDestroy.Clear();
            }
        }

        private void DispatchMessages()
        {
            // Snapshot dos receptores antes de despachar (mensagens podem alterar a cena).
            int guard = 0;
            while (_messages.Count > 0 && guard++ < 1000)
            {
                var message = _messages.Dequeue();
                MessageSent?.Invoke(message);
                foreach (var obj in AllObjects(_objects))
                    foreach (var component in obj.Components.ToArray())
                        if (component is Messaging.IMessageReceiver receiver)
                            receiver.OnMessage(message);
            }
        }

        private static System.Collections.Generic.IEnumerable<GameObject> AllObjects(
            System.Collections.Generic.IEnumerable<GameObject> objects)
        {
            foreach (var obj in objects.ToArray())
            {
                yield return obj;
                foreach (var child in AllObjects(obj.Children))
                    yield return child;
            }
        }

        public void Draw(ISceneDrawing drawing)
        {
            // Passe de mundo: objetos presos ao mundo/câmera, por z-order global (SortOrder),
            // estável — empates mantêm a ordem da cena. Objetos HUD ficam para o passe de tela.
            foreach (var obj in VisibleInDrawOrder())
                if (!obj.EffectiveScreenSpace)
                    obj.DrawSelf(drawing);
        }

        /// <summary>Passe de tela (HUD): objetos marcados como <see cref="GameObject.ScreenSpace"/>,
        /// desenhados fora da transformação de câmera. Chamado num segundo SpriteBatch.</summary>
        public void DrawScreenSpace(ISceneDrawing drawing)
        {
            foreach (var obj in VisibleInDrawOrder())
                if (obj.EffectiveScreenSpace)
                    obj.DrawSelf(drawing);
        }

        /// <summary>Objetos visíveis (respeitando ancestrais), ordenados por SortOrder.</summary>
        public IEnumerable<GameObject> VisibleInDrawOrder()
        {
            var flat = new List<GameObject>();
            Collect(_objects, flat);
            // Camada grossa primeiro, depois z-order fino (ou Y de mundo se tiver YSort);
            // ordenação estável mantém empates.
            return flat.OrderBy(o => o.RenderLayer).ThenBy(SortKey);

            static float SortKey(GameObject o)
            {
                foreach (var component in o.Components)
                    if (component is Components.YSort ys)
                        return ys.SortKey;
                return o.SortOrder;
            }

            static void Collect(IEnumerable<GameObject> objects, List<GameObject> into)
            {
                foreach (var obj in objects)
                {
                    if (!obj.IsVisible)
                        continue; // subárvore invisível não desenha
                    into.Add(obj);
                    Collect(obj.Children, into);
                }
            }
        }
    }
}
