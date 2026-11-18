using System;
using System.Collections.Generic;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Components.Custom;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Input;
using DreamBit.Engine.Levels;
using DreamBit.Engine.Physics;
using DreamBit.Engine.Serialization;
using Microsoft.Xna.Framework;
using GameInput = DreamBit.Engine.Input.Input;

namespace DreamBit.Engine.AI
{
    /// <summary>Consultas de terreno para IA de plataforma (chão à frente, parede à frente).</summary>
    public static class PlatformSense
    {
        /// <summary>Há chão sob o ponto (x, feet) dentro de <paramref name="depth"/> px (sólidos ou ledges)?</summary>
        public static bool GroundAt(Scene? scene, GameObject self, float x, float feet, float depth)
        {
            if (scene == null) return false;
            var solids = SolidQuery.Collect(scene, self, out _);
            foreach (var b in solids)
                if (x >= b.Min.X && x <= b.Max.X && b.Min.Y >= feet - 2f && b.Min.Y <= feet + depth)
                    return true;
            return !float.IsNaN(LedgePhysics.FindLanding(scene.Ledges, x, feet - 2f, feet + depth));
        }

        /// <summary>Há parede logo à frente (na altura do corpo)?</summary>
        public static bool WallAhead(Scene? scene, GameObject self, PlatformerController pc, int dir, float probe = 6f)
        {
            if (scene == null || dir == 0) return false;
            var p = self.Transform.WorldPosition;
            float x = p.X + dir * (pc.HalfWidth + probe);
            float top = p.Y - pc.CurrentHalfHeight + 4f, bottom = p.Y + pc.CurrentHalfHeight - 4f;
            foreach (var b in SolidQuery.Collect(scene, self, out _))
                if (x >= b.Min.X && x <= b.Max.X && bottom > b.Min.Y && top < b.Max.Y)
                    return true;
            return false;
        }

        internal static GameObject? FindByTagOrName(Scene? scene, string key, GameObject? exclude = null)
        {
            if (scene == null || string.IsNullOrWhiteSpace(key)) return null;
            return Bodies.All(scene.Objects).FirstOrDefault(o => o != exclude && (o.HasTag(key) || o.Name == key));
        }
    }

    /// <summary>
    /// Companheiro controlado pela IA (o personagem que não está com o jogador): segue o líder,
    /// pula quando ele está acima, em paredes e buracos, e teleporta para perto se ficar longe
    /// demais. Dirige o PlatformerController do objeto por uma <see cref="VirtualInput"/>.
    /// </summary>
    [GameComponent("IA/Platformer Follower", Id = "dreambit.follower")]
    public sealed class PlatformerFollower : SceneComponent
    {
        private readonly VirtualInput _input = new();
        private float _stuckTime;
        private float _lastX;

        public override string DisplayName => "Platformer Follower";

        /// <summary>Tag ou nome do líder.</summary>
        public string Leader { get; set; } = "Player";

        /// <summary>Distância horizontal a partir da qual começa a andar.</summary>
        public float FollowDistance { get; set; } = 110f;

        /// <summary>Distância em que para.</summary>
        public float StopDistance { get; set; } = 60f;

        /// <summary>Teleporta para trás do líder se passar desta distância (0 = nunca).</summary>
        public float TeleportDistance { get; set; } = 900f;

        /// <summary>Pula quando o líder está pelo menos isso acima.</summary>
        public float JumpHeightTrigger { get; set; } = 70f;

        /// <summary>Usa o dash para alcançar quando estiver longe (se tiver a habilidade).</summary>
        public bool DashToCatchUp { get; set; } = true;

        /// <summary>A IA está dirigindo o personagem agora (o PartySwitcher liga/desliga).</summary>
        public bool Active { get; set; } = true;

        /// <summary>Fonte de input da IA (para inspeção/testes).</summary>
        public VirtualInput Input => _input;

        protected internal override void OnPlayStarted() => _stuckTime = 0f;

        protected internal override void Update(GameTime gameTime)
        {
            var pc = Owner.Components.OfType<PlatformerController>().FirstOrDefault();
            if (pc == null) return;
            if (!Active)
            {
                if (pc.InputOverride == _input) pc.InputOverride = null;
                return;
            }
            pc.InputOverride = _input;
            _input.ReleaseAll();

            var leader = PlatformSense.FindByTagOrName(Owner.Scene, Leader, Owner);
            if (leader == null) return;
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var me = Owner.Transform.WorldPosition;
            var target = leader.Transform.WorldPosition;
            var delta = target - me;

            if (TeleportDistance > 0f && delta.Length() > TeleportDistance)
            {
                var leaderPc = leader.Components.OfType<PlatformerController>().FirstOrDefault();
                int behind = leaderPc != null ? -leaderPc.Facing : Math.Sign(-delta.X);
                pc.Teleport(target + new Vector2(behind * StopDistance, 0f) - (Owner.Parent?.Transform.WorldPosition ?? Vector2.Zero));
                return;
            }

            int dir = Math.Abs(delta.X) > (pc.IsMovingHorizontally ? StopDistance : FollowDistance) ? Math.Sign(delta.X) : 0;
            _input.SetAxes(dir);

            if (dir != 0)
            {
                // Preso (andando sem sair do lugar): tenta pular.
                _stuckTime = Math.Abs(me.X - _lastX) < 0.5f ? _stuckTime + dt : 0f;
                float feet = me.Y + pc.CurrentHalfHeight;
                bool gapAhead = pc.Grounded && !PlatformSense.GroundAt(Owner.Scene, Owner, me.X + dir * (pc.HalfWidth + 20f), feet, pc.HalfHeight * 2f);
                bool wall = PlatformSense.WallAhead(Owner.Scene, Owner, pc, dir);
                bool leaderAbove = -delta.Y > JumpHeightTrigger;
                bool leaderBelowGap = gapAhead && delta.Y > pc.HalfHeight; // o líder desceu: deixa cair
                if (pc.Grounded && !leaderBelowGap && (wall || gapAhead || leaderAbove || _stuckTime > 0.25f))
                    _input.Tap("Jump");
                else if (!pc.Grounded && pc.Velocity.Y > 0f && (wall || leaderAbove) && pc.Has(MovementAbilities.DoubleJump))
                    _input.Tap("Jump");
                if (DashToCatchUp && Math.Abs(delta.X) > FollowDistance * 4f && pc.Has(MovementAbilities.Dash))
                    _input.Tap("Dash");
                // Mantém o pulo apertado para a altura máxima.
                if (!pc.Grounded && pc.Velocity.Y < 0f) _input.Set("Jump", true);
            }
            else if (!pc.Grounded && pc.Velocity.Y < 0f && -delta.Y > 0f)
                _input.Set("Jump", true);
            _lastX = me.X;
        }
    }

    /// <summary>
    /// Grupo de personagens (ex.: Ruber e Aloine). Sozinho, o jogador controla um e a IA o
    /// outro (<see cref="PlatformerFollower"/>); a ação "Swap" troca. Em co-op cada membro tem
    /// seu jogador. Com <see cref="AllowDropIn"/>, o P2 entra apertando Confirm/Start no controle 2.
    /// </summary>
    [GameComponent("IA/Party Switcher", Id = "dreambit.party")]
    public sealed class PartySwitcher : SceneComponent
    {
        public override string DisplayName => "Party Switcher";

        /// <summary>Membros (tags ou nomes), na ordem; o primeiro começa com o jogador.</summary>
        public string Members { get; set; } = "Ruber,Aloine";

        /// <summary>Índice do membro controlado pelo P1 (modo solo).</summary>
        public int ActiveIndex { get; set; }

        /// <summary>Co-op local: cada membro com um jogador (P1, P2…).</summary>
        public bool Coop { get; set; }

        /// <summary>P2 entra/sai do co-op pelo controle 2 (Confirm/Start entra; segurar Cancel sai).</summary>
        public bool AllowDropIn { get; set; } = true;

        /// <summary>No co-op, P1 e P2 dividem o teclado (P1 esquerdo, P2 direito).</summary>
        public bool SharedKeyboardCoop { get; set; }

        /// <summary>Ação que troca de personagem (lida do P1).</summary>
        public string SwapAction { get; set; } = "Swap";

        /// <summary>Disparado ao trocar (membro que passou a ser controlado pelo P1).</summary>
        public event Action<GameObject>? Swapped;

        /// <summary>Disparado ao entrar/sair do co-op.</summary>
        public event Action<bool>? CoopChanged;

        /// <summary>Membro atualmente com o P1.</summary>
        public GameObject? ActiveMember => MemberObjects().ElementAtOrDefault(ActiveIndex);

        public IReadOnlyList<GameObject> MemberObjects()
        {
            var list = new List<GameObject>();
            foreach (var key in Members.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                if (PlatformSense.FindByTagOrName(Owner?.Scene, key) is { } o)
                    list.Add(o);
            return list;
        }

        public void SwapTo(int index)
        {
            var members = MemberObjects();
            if (members.Count == 0) return;
            ActiveIndex = ((index % members.Count) + members.Count) % members.Count;
            Apply(members);
            Swapped?.Invoke(members[ActiveIndex]);
        }

        public void SetCoop(bool coop)
        {
            if (Coop == coop) return;
            Coop = coop;
            if (coop) GameInput.Players.ConfigureCoop(SharedKeyboardCoop);
            else GameInput.Players.ConfigureSinglePlayer();
            Apply(MemberObjects());
            CoopChanged?.Invoke(coop);
        }

        protected internal override void OnPlayStarted()
        {
            if (Coop) GameInput.Players.ConfigureCoop(SharedKeyboardCoop);
            Apply(MemberObjects());
        }

        protected internal override void Update(GameTime gameTime)
        {
            var members = MemberObjects();
            if (members.Count == 0) return;

            if (AllowDropIn)
            {
                var pad2 = GameInput.PadNow(1);
                var pad2Prev = GameInput.PadPrev(1);
                bool join = pad2.IsConnected && (Pressed(pad2, pad2Prev, Microsoft.Xna.Framework.Input.Buttons.Start) || Pressed(pad2, pad2Prev, Microsoft.Xna.Framework.Input.Buttons.A));
                if (!Coop && join && members.Count > 1) SetCoop(true);
                else if (Coop && pad2.IsConnected && Pressed(pad2, pad2Prev, Microsoft.Xna.Framework.Input.Buttons.Back)) SetCoop(false);
            }

            if (!Coop && GameInput.Players[0].JustPressed(SwapAction))
                SwapTo(ActiveIndex + 1);
        }

        private static bool Pressed(Microsoft.Xna.Framework.Input.GamePadState now, Microsoft.Xna.Framework.Input.GamePadState prev, Microsoft.Xna.Framework.Input.Buttons b)
            => now.IsButtonDown(b) && !prev.IsButtonDown(b);

        private void Apply(IReadOnlyList<GameObject> members)
        {
            if (members.Count == 0) return;
            ActiveIndex = Math.Clamp(ActiveIndex, 0, members.Count - 1);
            var leader = members[ActiveIndex];
            for (int i = 0; i < members.Count; i++)
            {
                var m = members[i];
                var pc = m.Components.OfType<PlatformerController>().FirstOrDefault();
                var follower = m.Components.OfType<PlatformerFollower>().FirstOrDefault();
                if (Coop)
                {
                    int player = i == ActiveIndex ? 0 : (i < ActiveIndex ? i + 1 : i);
                    if (pc != null) { pc.PlayerIndex = Math.Min(player, GameInput.MaxPlayers - 1); pc.InputOverride = null; }
                    if (follower != null) follower.Active = false;
                }
                else if (i == ActiveIndex)
                {
                    if (pc != null) { pc.PlayerIndex = 0; pc.InputOverride = null; }
                    if (follower != null) follower.Active = false;
                }
                else
                {
                    if (follower == null) follower = m.AddComponent(new PlatformerFollower());
                    follower.Leader = string.IsNullOrEmpty(leader.Tag) ? leader.Name : leader.Tag;
                    follower.Active = true;
                    if (pc != null) pc.PlayerIndex = -1;
                }
            }
        }
    }

    /// <summary>
    /// Inimigo de plataforma: patrulha (vira em paredes e bordas) e persegue um alvo dentro do
    /// alcance. Dirige o PlatformerController do objeto por uma <see cref="VirtualInput"/>.
    /// </summary>
    [GameComponent("IA/Patrol Enemy", Id = "dreambit.patrol")]
    public sealed class PatrolEnemy : SceneComponent
    {
        private readonly VirtualInput _input = new();
        private int _dir = 1;
        private float _wait;

        public override string DisplayName => "Patrol Enemy";

        /// <summary>Alvo perseguido (tags/nomes separados por vírgula; vazio = só patrulha).</summary>
        public string ChaseTarget { get; set; } = "Player";
        public float ChaseRange { get; set; } = 320f;
        /// <summary>Diferença de altura máxima para perseguir.</summary>
        public float ChaseHeight { get; set; } = 120f;
        /// <summary>Fração da velocidade ao patrulhar (0..1).</summary>
        public float PatrolSpeed { get; set; } = 0.5f;
        /// <summary>Pausa ao virar (s).</summary>
        public float TurnPause { get; set; } = 0.4f;
        /// <summary>Não cai de bordas.</summary>
        public bool AvoidEdges { get; set; } = true;
        /// <summary>Pula obstáculos ao perseguir.</summary>
        public bool JumpWhenChasing { get; set; } = true;

        /// <summary>Alvo atual (null = patrulhando).</summary>
        public GameObject? Target { get; private set; }

        public int Direction => _dir;

        protected internal override void Update(GameTime gameTime)
        {
            var pc = Owner.Components.OfType<PlatformerController>().FirstOrDefault();
            if (pc == null) return;
            pc.InputOverride = _input;
            _input.ReleaseAll();
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var me = Owner.Transform.WorldPosition;

            Target = null;
            foreach (var key in ChaseTarget.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var t = PlatformSense.FindByTagOrName(Owner.Scene, key, Owner);
                if (t == null) continue;
                var d = t.Transform.WorldPosition - me;
                if (Math.Abs(d.X) <= ChaseRange && Math.Abs(d.Y) <= ChaseHeight && (Target == null || d.Length() < (Target.Transform.WorldPosition - me).Length()))
                    Target = t;
            }

            if (_wait > 0f) { _wait -= dt; return; }

            float feet = me.Y + pc.CurrentHalfHeight;
            if (Target != null)
            {
                float dx = Target.Transform.WorldPosition.X - me.X;
                _dir = Math.Abs(dx) > 4f ? Math.Sign(dx) : _dir;
                bool edge = AvoidEdges && pc.Grounded && !PlatformSense.GroundAt(Owner.Scene, Owner, me.X + _dir * (pc.HalfWidth + 8f), feet, pc.HalfHeight * 2f);
                bool wall = PlatformSense.WallAhead(Owner.Scene, Owner, pc, _dir);
                if (Math.Abs(dx) > pc.HalfWidth && !(edge && !JumpWhenChasing))
                    _input.SetAxes(_dir);
                if (JumpWhenChasing && pc.Grounded && (wall || edge))
                    _input.Tap("Jump");
                return;
            }

            bool turn = PlatformSense.WallAhead(Owner.Scene, Owner, pc, _dir) ||
                        (AvoidEdges && pc.Grounded && !PlatformSense.GroundAt(Owner.Scene, Owner, me.X + _dir * (pc.HalfWidth + 8f), feet, pc.HalfHeight * 2f));
            if (turn)
            {
                _dir = -_dir;
                _wait = TurnPause;
                return;
            }
            _input.SetAxes(_dir * Math.Clamp(PatrolSpeed, 0f, 1f));
        }
    }

    /// <summary>
    /// Projétil (bola de fogo, flecha, rajada de vento): anda com velocidade e gravidade, dá dano
    /// em Hurtboxes de outro time, quebra <see cref="Breakable"/> do tipo <see cref="BreakKind"/>
    /// e some ao bater num sólido ou ao fim da vida.
    /// </summary>
    [GameComponent("Combate/Projectile", Id = "dreambit.projectile")]
    public sealed class Projectile : SceneComponent
    {
        private float _age;

        public override string DisplayName => "Projectile";

        public Vector2 Velocity { get; set; } = new(600, 0);
        public float Gravity { get; set; }
        public float Lifetime { get; set; } = 2f;
        public float Damage { get; set; } = 20f;
        /// <summary>Time do atirador (não acerta o próprio time).</summary>
        public int Team { get; set; }
        /// <summary>Raio de colisão.</summary>
        public float Radius { get; set; } = 12f;
        /// <summary>Tipo de quebra (ex.: "Fire" queima vinhas Breakable com BreakBy "Fire").</summary>
        public string BreakKind { get; set; } = "";
        public bool DestroyOnSolid { get; set; } = true;
        /// <summary>Atravessa inimigos (não some ao acertar).</summary>
        public bool Piercing { get; set; }
        /// <summary>Gira o objeto na direção do movimento.</summary>
        public bool AlignToVelocity { get; set; } = true;
        /// <summary>Mensagem enviada ao acertar algo.</summary>
        public string SendOnHit { get; set; } = "";

        public event Action<Projectile, GameObject?>? Hit;

        private readonly HashSet<GameObject> _hits = new();

        protected internal override void OnPlayStarted() { _age = 0f; _hits.Clear(); }

        protected internal override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (dt <= 0f || Owner.Scene == null) return;
            _age += dt;
            if (_age >= Lifetime) { Owner.Scene.Destroy(Owner); return; }

            Velocity += new Vector2(0f, Gravity * dt);
            Owner.Transform.Position += Velocity * dt;
            if (AlignToVelocity && Velocity != Vector2.Zero)
                Owner.Transform.Rotation = (float)Math.Atan2(Velocity.Y, Velocity.X);

            var p = Owner.Transform.WorldPosition;
            var me = new Geometry.Aabb(p - new Vector2(Radius), p + new Vector2(Radius));

            foreach (var hb in Bodies.All(Owner.Scene.Objects).SelectMany(o => o.Components.OfType<Hurtbox>()).ToList())
            {
                if (hb.Team == Team || !hb.Enabled || _hits.Contains(hb.Owner)) continue;
                var bounds = hb.Bounds();
                if (!me.Overlaps(bounds)) continue;
                _hits.Add(hb.Owner);
                hb.Health?.Damage(Damage);
                OnHit(hb.Owner);
                if (!Piercing) { Owner.Scene.Destroy(Owner); return; }
            }

            var solids = SolidQuery.Collect(Owner.Scene, Owner, out var owners);
            for (int i = 0; i < solids.Count; i++)
            {
                var b = solids[i];
                if (p.X + Radius < b.Min.X || p.X - Radius > b.Max.X || p.Y + Radius < b.Min.Y || p.Y - Radius > b.Max.Y) continue;
                if (owners[i] is { } other && !string.IsNullOrEmpty(BreakKind))
                    Breakable.TryBreak(other, BreakKind, Owner);
                OnHit(owners[i]);
                if (DestroyOnSolid) { Owner.Scene.Destroy(Owner); return; }
            }
        }

        private void OnHit(GameObject? what)
        {
            Hit?.Invoke(this, what);
            if (!string.IsNullOrEmpty(SendOnHit)) Owner.Scene?.Send(SendOnHit, Owner);
        }

        /// <summary>Cria um projétil a partir de um prefab (.dbprefab) na posição/velocidade dadas.</summary>
        public static GameObject Spawn(Scene scene, string prefabPath, Vector2 position, Vector2 velocity, int team = 0)
        {
            var obj = SceneSerializer.LoadPrefab(Assets.AssetPaths.Resolve(prefabPath));
            return Place(scene, obj, position, velocity, team);
        }

        /// <summary>Cria um projétil simples (sem prefab).</summary>
        public static GameObject Spawn(Scene scene, Vector2 position, Vector2 velocity, int team = 0, float damage = 20f, string breakKind = "")
        {
            var obj = new GameObject("Projetil");
            obj.AddComponent(new Projectile { Damage = damage, BreakKind = breakKind });
            return Place(scene, obj, position, velocity, team);
        }

        private static GameObject Place(Scene scene, GameObject obj, Vector2 position, Vector2 velocity, int team)
        {
            obj.Transform.Position = position;
            var proj = obj.Components.OfType<Projectile>().FirstOrDefault() ?? obj.AddComponent(new Projectile());
            proj.Velocity = velocity;
            proj.Team = team;
            scene.Add(obj);
            return obj;
        }
    }
}
