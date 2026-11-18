using System;
using System.Collections.Generic;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Components.Custom;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using DreamBit.Engine.Saving;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Levels
{
    /// <summary>Utilitários de sobreposição entre objetos (zonas, portas, coletáveis).</summary>
    public static class Bodies
    {
        /// <summary>Meia-extensão do corpo de um objeto: Platformer → BoxCollider → Hurtbox → Sprite → 24.</summary>
        public static Vector2 HalfSize(GameObject obj)
        {
            foreach (var c in obj.Components)
                if (c is PlatformerController p) return new Vector2(p.HalfWidth, p.CurrentHalfHeight);
            foreach (var c in obj.Components)
                if (c is BoxCollider b) return b.Size / 2f;
            foreach (var c in obj.Components)
                if (c is Hurtbox h) return new Vector2(h.Width, h.Height) / 2f;
            foreach (var c in obj.Components)
                if (c is SpriteRenderer s) return s.Size / 2f;
            return new Vector2(24, 24);
        }

        public static bool Overlaps(Vector2 center, Vector2 size, GameObject other)
        {
            var h1 = size / 2f;
            var c2 = other.Transform.WorldPosition;
            var h2 = HalfSize(other);
            return Math.Abs(center.X - c2.X) < h1.X + h2.X && Math.Abs(center.Y - c2.Y) < h1.Y + h2.Y;
        }

        /// <summary>True se o objeto bate com o filtro de tags (vírgulas). "Player" também aceita
        /// quem tem PlatformerController controlado por jogador; vazio aceita qualquer Platformer.</summary>
        public static bool Matches(GameObject obj, string tags)
        {
            if (string.IsNullOrWhiteSpace(tags))
                return obj.Components.OfType<PlatformerController>().Any();
            foreach (var t in tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (obj.HasTag(t)) return true;
                if (string.Equals(t, "Player", StringComparison.OrdinalIgnoreCase) &&
                    obj.Components.OfType<PlatformerController>().Any(p => p.InputOverride == null || p.PlayerIndex >= 0))
                    return true;
                if (string.Equals(t, "Pushable", StringComparison.OrdinalIgnoreCase) && obj.Components.OfType<Pushable>().Any())
                    return true;
            }
            return false;
        }

        public static IEnumerable<GameObject> All(IEnumerable<GameObject> objects)
        {
            foreach (var o in objects)
            {
                yield return o;
                foreach (var c in All(o.Children))
                    yield return c;
            }
        }

        /// <summary>Objetos da cena que batem com o filtro e se sobrepõem à área.</summary>
        public static IEnumerable<GameObject> InArea(SceneComponent zone, Vector2 size, string tags)
        {
            var scene = zone.Owner.Scene;
            if (scene == null) yield break;
            var center = zone.Owner.Transform.WorldPosition;
            foreach (var o in All(scene.Objects))
                if (o != zone.Owner && Matches(o, tags) && Overlaps(center, size, o))
                    yield return o;
        }

        internal static void DrawArea(ISceneDrawing drawing, GameObject owner, Vector2 size, Color color)
        {
            if (DebugDraw.Zones)
                drawing.DrawQuad(Matrix.CreateTranslation(owner.Transform.WorldPosition.X, owner.Transform.WorldPosition.Y, 0f), size, color);
        }
    }

    /// <summary>Ponto de chegada numa sala (usado por portas e saves).</summary>
    [GameComponent("Mundo/Spawn Point", Id = "dreambit.spawn")]
    public sealed class SpawnPoint : SceneComponent
    {
        public override string DisplayName => "Spawn Point";

        /// <summary>Nome do ponto (as portas apontam para ele).</summary>
        public string Name { get; set; } = "default";

        /// <summary>Direção inicial (+1 direita, -1 esquerda).</summary>
        public int Facing { get; set; } = 1;

        /// <summary>Distância horizontal entre os membros do grupo ao chegar.</summary>
        public float PartySpacing { get; set; } = 48f;

        protected internal override void Draw(ISceneDrawing drawing)
            => Bodies.DrawArea(drawing, Owner, new Vector2(24, 48), new Color(120, 200, 255) * 0.35f);
    }

    /// <summary>Marca o objeto para sobreviver à troca de cena (o personagem, o grupo, o HUD).
    /// Ao entrar numa cena que já tem um objeto persistente com o mesmo nome, o da cena é descartado.</summary>
    [GameComponent("Mundo/Persistent", Id = "dreambit.persistent")]
    public sealed class Persistent : SceneComponent
    {
        public override string DisplayName => "Persistent";

        /// <summary>Se é posicionado no spawn ao chegar (membros do grupo).</summary>
        public bool PlaceAtSpawn { get; set; } = true;

        /// <summary>Ordem no grupo ao posicionar (0 = à frente).</summary>
        public int PartyOrder { get; set; }
    }

    /// <summary>Porta/passagem para outra sala (cena), chegando num <see cref="SpawnPoint"/>.</summary>
    [GameComponent("Mundo/Room Door", Id = "dreambit.door")]
    public sealed class RoomDoor : SceneComponent
    {
        private bool _used;
        private bool _wasInside = true;

        public override string DisplayName => "Room Door";

        public Vector2 Size { get; set; } = new(64, 128);

        /// <summary>Arquivo da cena de destino (relativo à cena atual), ex.: "Floresta_02.dbscene".</summary>
        [AssetPath("*.dbscene")]
        public string TargetScene { get; set; } = "";

        /// <summary>Nome do SpawnPoint no destino.</summary>
        public string TargetSpawn { get; set; } = "default";

        /// <summary>Quem atravessa (tags separadas por vírgula; vazio = qualquer Platformer).</summary>
        public string Tag { get; set; } = "Player";

        /// <summary>Ação para entrar (ex.: "MoveUp", "Action"); vazio = ao encostar.</summary>
        public string RequireAction { get; set; } = "";

        /// <summary>Flag do WorldState necessária (porta trancada); vazio = sempre aberta.</summary>
        public string RequiredFlag { get; set; } = "";

        /// <summary>Mensagem enviada ao tentar passar trancada.</summary>
        public string LockedMessage { get; set; } = "door_locked";

        public bool IsLocked => !string.IsNullOrEmpty(RequiredFlag) && !WorldState.HasFlag(RequiredFlag);

        protected internal override void OnPlayStarted()
        {
            _used = false;
            _wasInside = true; // quem nasce dentro da porta precisa sair antes de usá-la
        }

        protected internal override void Update(GameTime gameTime)
        {
            if (_used || string.IsNullOrEmpty(TargetScene) || Owner.Scene == null)
                return;
            var who = Bodies.InArea(this, Size, Tag).FirstOrDefault();
            bool inside = who != null;
            bool entered = inside && !_wasInside;
            _wasInside = inside;
            if (who == null)
                return;

            bool wants = string.IsNullOrEmpty(RequireAction)
                ? entered || who.Components.OfType<PlatformerController>().Any(p => Math.Abs(p.Velocity.X) > 1f)
                : (who.Components.OfType<Input.IInputControlled>().FirstOrDefault()?.InputSource ?? Input.Input.Global).JustPressed(RequireAction);
            if (!wants)
                return;

            if (IsLocked)
            {
                if (!string.IsNullOrEmpty(LockedMessage)) Owner.Scene.Send(LockedMessage, Owner);
                return;
            }
            _used = true;
            WorldNavigator.GoTo(Owner.Scene, TargetScene, TargetSpawn);
        }

        protected internal override void Draw(ISceneDrawing drawing)
            => Bodies.DrawArea(drawing, Owner, Size, new Color(120, 230, 150) * 0.25f);
    }

    /// <summary>
    /// Zona de câmera: enquanto o alvo da câmera está dentro, a câmera fica limitada ao retângulo
    /// (salas de metroidvania). Zonas menores têm prioridade sobre maiores.
    /// </summary>
    [GameComponent("Mundo/Camera Zone", Id = "dreambit.camerazone")]
    public sealed class CameraZone : SceneComponent
    {
        public override string DisplayName => "Camera Zone";

        public Vector2 Size { get; set; } = new(1920, 1080);

        /// <summary>Retângulo da zona em coordenadas de mundo.</summary>
        public (Vector2 Min, Vector2 Max) WorldRect
        {
            get
            {
                var c = Owner.Transform.WorldPosition;
                return (c - Size / 2f, c + Size / 2f);
            }
        }

        protected internal override void Update(GameTime gameTime)
        {
            var scene = Owner.Scene;
            if (scene == null) return;
            var zones = Bodies.All(scene.Objects).SelectMany(o => o.Components.OfType<CameraZone>()).Where(z => z.Enabled).ToList();
            if (zones.Count == 0 || zones[0] != this) return; // só a primeira zona resolve todas

            foreach (var cam in Bodies.All(scene.Objects).SelectMany(o => o.Components.OfType<CameraComponent>()))
            {
                var target = cam.Targets.FirstOrDefault();
                if (target == null) continue;
                var p = target.Transform.WorldPosition;
                CameraZone? best = null;
                foreach (var z in zones)
                {
                    var (min, max) = z.WorldRect;
                    if (p.X < min.X || p.X > max.X || p.Y < min.Y || p.Y > max.Y) continue;
                    if (best == null || z.Size.X * z.Size.Y < best.Size.X * best.Size.Y) best = z;
                }
                cam.BoundsOverride = best?.WorldRect;
            }
        }

        protected internal override void Draw(ISceneDrawing drawing)
            => Bodies.DrawArea(drawing, Owner, Size, new Color(255, 220, 120) * 0.06f);
    }

    /// <summary>Sala do mapa-múndi: ao ser visitada, fica descoberta (WorldState "map.&lt;RoomId&gt;").</summary>
    [GameComponent("Mundo/Map Room", Id = "dreambit.maproom")]
    public sealed class MapRoom : SceneComponent
    {
        public override string DisplayName => "Map Room";

        /// <summary>Identificador único da sala no mapa.</summary>
        public string RoomId { get; set; } = "";

        /// <summary>Área da sala no mundo (também usada para desenhar o mapa).</summary>
        public Vector2 Size { get; set; } = new(1920, 1080);

        /// <summary>Região/área (ex.: "Bosque dos Murmúrios") para agrupar no mapa.</summary>
        public string Region { get; set; } = "";

        protected internal override void Update(GameTime gameTime)
        {
            if (string.IsNullOrEmpty(RoomId) || WorldMap.IsDiscovered(RoomId)) return;
            if (Bodies.InArea(this, Size, "Player").Any())
                WorldMap.Discover(RoomId);
        }
    }

    /// <summary>Salas descobertas do mapa-múndi (persistidas no WorldState).</summary>
    public static class WorldMap
    {
        public static bool IsDiscovered(string roomId) => WorldState.HasFlag("map." + roomId);
        public static void Discover(string roomId) => WorldState.SetFlag("map." + roomId);

        /// <summary>Salas desta cena (para desenhar o mapa).</summary>
        public static IEnumerable<MapRoom> Rooms(Scene scene)
            => Bodies.All(scene.Objects).SelectMany(o => o.Components.OfType<MapRoom>());
    }

    /// <summary>
    /// Coletável/item: ao ser tocado, grava flags/contadores, libera habilidades de movimento
    /// no personagem que pegou e some (com <see cref="PersistKey"/>, não volta).
    /// </summary>
    [GameComponent("Mundo/Pickup", Id = "dreambit.pickup")]
    public sealed class Pickup : SceneComponent
    {
        public override string DisplayName => "Pickup";

        public Vector2 Size { get; set; } = new(40, 40);

        /// <summary>Quem pode pegar (tags; vazio = qualquer Platformer).</summary>
        public string Tag { get; set; } = "Player";

        /// <summary>Flag gravada ao pegar; se já existir, o item não aparece.</summary>
        public string PersistKey { get; set; } = "";

        /// <summary>Contador somado (ex.: "moedas", "fragmentos").</summary>
        public string CounterKey { get; set; } = "";
        public int CounterAmount { get; set; } = 1;

        /// <summary>Habilidades liberadas (nomes de MovementAbilities separados por vírgula).</summary>
        [Tooltip("Ex.: DoubleJump, Glide (habilidade da coruja)")]
        public string UnlockAbilities { get; set; } = "";

        /// <summary>Libera para todos os Platformers que batem com a tag (grupo), não só quem pegou.</summary>
        public bool UnlockForWholeParty { get; set; }

        /// <summary>Mensagem enviada ao pegar.</summary>
        public string SendOnPickup { get; set; } = "pickup";

        /// <summary>Disparado ao pegar (item, quem pegou).</summary>
        public event Action<Pickup, GameObject>? PickedUp;

        public bool Collected { get; private set; }

        protected internal override void OnPlayStarted()
        {
            Collected = !string.IsNullOrEmpty(PersistKey) && WorldState.HasFlag(PersistKey);
            if (Collected) Owner.IsVisible = false;
        }

        protected internal override void Update(GameTime gameTime)
        {
            if (Collected || Owner.Scene == null) return;
            var who = Bodies.InArea(this, Size, Tag).FirstOrDefault();
            if (who != null) Collect(who);
        }

        public void Collect(GameObject who)
        {
            if (Collected) return;
            Collected = true;
            if (!string.IsNullOrEmpty(PersistKey)) WorldState.SetFlag(PersistKey);
            if (!string.IsNullOrEmpty(CounterKey)) WorldState.AddCounter(CounterKey, CounterAmount);

            var abilities = ParseAbilities(UnlockAbilities);
            if (abilities != MovementAbilities.None)
            {
                var targets = UnlockForWholeParty && Owner.Scene != null
                    ? Bodies.All(Owner.Scene.Objects).Where(o => Bodies.Matches(o, Tag))
                    : new[] { who };
                foreach (var pc in targets.SelectMany(o => o.Components.OfType<PlatformerController>()))
                    pc.Unlock(abilities);
            }

            PickedUp?.Invoke(this, who);
            if (!string.IsNullOrEmpty(SendOnPickup)) Owner.Scene?.Send(SendOnPickup, Owner);
            Owner.Scene?.Destroy(Owner);
        }

        public static MovementAbilities ParseAbilities(string names)
        {
            var flags = MovementAbilities.None;
            foreach (var part in (names ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                if (Enum.TryParse<MovementAbilities>(part, true, out var f)) flags |= f;
            return flags;
        }
    }

    /// <summary>Sinais de runtime (não persistentes) usados por interruptores e portões.</summary>
    public static class Signals
    {
        private static readonly HashSet<string> _on = new(StringComparer.Ordinal);

        public static bool IsOn(string name) => _on.Contains(name) || WorldState.HasFlag(name);

        public static void Set(string name, bool on, bool persist = false)
        {
            if (string.IsNullOrEmpty(name)) return;
            if (persist) WorldState.SetFlag(name, on);
            else if (on) _on.Add(name);
            else _on.Remove(name);
        }

        /// <summary>Limpa os sinais de runtime (troca de cena).</summary>
        public static void ClearRuntime() => _on.Clear();
    }

    public enum SwitchMode { PressurePlate, Lever, Touch }

    /// <summary>
    /// Interruptor: placa de pressão (ligada enquanto algo está em cima — puzzles cooperativos),
    /// alavanca (alterna com a ação) ou toque (liga de vez). Liga o sinal <see cref="Signal"/>.
    /// </summary>
    [GameComponent("Mundo/Switch", Id = "dreambit.switch")]
    public sealed class Switch : SceneComponent
    {
        public override string DisplayName => "Switch";

        public SwitchMode Mode { get; set; } = SwitchMode.PressurePlate;

        /// <summary>Nome do sinal ligado/desligado.</summary>
        public string Signal { get; set; } = "";

        public Vector2 Size { get; set; } = new(64, 24);

        /// <summary>Quem ativa (tags; "Pushable" = caixotes; vazio = qualquer Platformer).</summary>
        public string Tag { get; set; } = "Player,Pushable";

        /// <summary>Ação da alavanca.</summary>
        public string LeverAction { get; set; } = "Action";

        /// <summary>Grava no WorldState (fica ligado depois de salvar).</summary>
        public bool Persist { get; set; }

        public bool IsOn { get; private set; }

        protected internal override void OnPlayStarted()
        {
            IsOn = Persist && WorldState.HasFlag(Signal);
            Signals.Set(Signal, IsOn, Persist);
        }

        protected internal override void Update(GameTime gameTime)
        {
            if (Owner.Scene == null || string.IsNullOrEmpty(Signal)) return;
            var inside = Bodies.InArea(this, Size, Tag).ToList();
            bool next = IsOn;
            switch (Mode)
            {
                case SwitchMode.PressurePlate:
                    next = inside.Count > 0;
                    break;
                case SwitchMode.Touch:
                    next = IsOn || inside.Count > 0;
                    break;
                case SwitchMode.Lever:
                    foreach (var who in inside)
                    {
                        var src = who.Components.OfType<Input.IInputControlled>().FirstOrDefault()?.InputSource;
                        if (src != null && src.JustPressed(LeverAction)) { next = !IsOn; break; }
                    }
                    break;
            }
            if (next == IsOn) return;
            IsOn = next;
            Signals.Set(Signal, IsOn, Persist);
            Owner.Scene.Send((IsOn ? "on:" : "off:") + Signal, Owner);
        }

        protected internal override void Draw(ISceneDrawing drawing)
            => Bodies.DrawArea(drawing, Owner, Size, (IsOn ? new Color(120, 255, 140) : new Color(255, 160, 80)) * 0.35f);
    }

    /// <summary>Portão/porta que abre quando os sinais/flags estão ligados (deslizando por
    /// <see cref="OpenOffset"/>) e fecha quando desligam (a menos que <see cref="StayOpen"/>).</summary>
    [GameComponent("Mundo/Gate", Id = "dreambit.gate")]
    public sealed class Gate : SceneComponent
    {
        private Vector2 _closed;
        private float _t;

        public override string DisplayName => "Gate";

        /// <summary>Sinais/flags que abrem (vírgulas).</summary>
        public string OpenWhen { get; set; } = "";

        /// <summary>Exige todos (true) ou basta um (false).</summary>
        public bool RequireAll { get; set; } = true;

        /// <summary>Deslocamento quando aberto (ex.: 0,-200 sobe).</summary>
        public Vector2 OpenOffset { get; set; } = new(0, -200);

        /// <summary>Tempo para abrir/fechar (s).</summary>
        public float Duration { get; set; } = 0.6f;

        /// <summary>Depois de aberto não fecha mais.</summary>
        public bool StayOpen { get; set; }

        public bool IsOpen => _t >= 1f;

        protected internal override void OnPlayStarted()
        {
            _closed = Owner.Transform.Position;
            _t = Evaluate() ? 1f : 0f;
            Apply();
        }

        protected internal override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            bool open = Evaluate() || (StayOpen && _t >= 1f);
            float step = Duration <= 0f ? 1f : dt / Duration;
            _t = MathHelper.Clamp(_t + (open ? step : -step), 0f, 1f);
            Apply();
        }

        private bool Evaluate()
        {
            var names = OpenWhen.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (names.Length == 0) return false;
            return RequireAll ? names.All(Signals.IsOn) : names.Any(Signals.IsOn);
        }

        private void Apply()
        {
            float e = _t * _t * (3f - 2f * _t); // smoothstep
            Owner.Transform.Position = _closed + OpenOffset * e;
        }
    }

    /// <summary>
    /// Ponto de save: com o jogador dentro e a ação apertada, salva no slot atual
    /// (<see cref="SaveSlots"/>), cura a <see cref="Health"/> e avisa a cena.
    /// </summary>
    [GameComponent("Mundo/Save Point", Id = "dreambit.savepoint")]
    public sealed class SavePoint : SceneComponent
    {
        public override string DisplayName => "Save Point";

        public Vector2 Size { get; set; } = new(96, 128);
        public string Tag { get; set; } = "Player";
        public string RequireAction { get; set; } = "MoveUp";

        /// <summary>Spawn usado ao carregar este save (vazio = posição do ponto).</summary>
        public string SpawnName { get; set; } = "";

        public bool HealOnSave { get; set; } = true;
        public string SendOnSave { get; set; } = "saved";

        protected internal override void Update(GameTime gameTime)
        {
            if (Owner.Scene == null) return;
            foreach (var who in Bodies.InArea(this, Size, Tag))
            {
                var src = who.Components.OfType<Input.IInputControlled>().FirstOrDefault()?.InputSource ?? Input.Input.Global;
                if (!src.JustPressed(RequireAction)) continue;
                if (HealOnSave)
                    foreach (var h in Bodies.All(Owner.Scene.Objects).Where(o => Bodies.Matches(o, Tag)).SelectMany(o => o.Components.OfType<Health>()))
                        h.Heal(h.Max);
                SaveSlots.Save(SaveSlots.CurrentSlot, WorldNavigator.CurrentSceneFile ?? "", string.IsNullOrEmpty(SpawnName) ? null : SpawnName,
                    string.IsNullOrEmpty(SpawnName) ? Owner.Transform.WorldPosition : null);
                if (!string.IsNullOrEmpty(SendOnSave)) Owner.Scene.Send(SendOnSave, Owner);
                return;
            }
        }

        protected internal override void Draw(ISceneDrawing drawing)
            => Bodies.DrawArea(drawing, Owner, Size, new Color(200, 140, 255) * 0.25f);
    }
}
