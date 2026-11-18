using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Saving;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Levels
{
    /// <summary>
    /// Navegação entre salas: guarda o spawn de destino, leva os objetos
    /// <see cref="Persistent"/> para a nova cena e posiciona o grupo no <see cref="SpawnPoint"/>.
    /// O <see cref="Hosting.SceneHost"/> chama <see cref="BeforeSceneSwap"/>/<see cref="AfterSceneLoaded"/>.
    /// </summary>
    public static class WorldNavigator
    {
        private static readonly List<GameObject> _carried = new();

        /// <summary>Spawn a usar na próxima cena carregada.</summary>
        public static string? PendingSpawn { get; set; }

        /// <summary>Posição explícita de chegada (load de save sem spawn), com prioridade sobre o spawn.</summary>
        public static Vector2? PendingPosition { get; set; }

        /// <summary>Arquivo da cena atual (relativo à raiz de assets, como foi pedido).</summary>
        public static string? CurrentSceneFile { get; set; }

        /// <summary>Disparado depois de posicionar o grupo na nova sala.</summary>
        public static event Action<Scene>? Arrived;

        /// <summary>Pede a troca para <paramref name="sceneFile"/>, chegando em <paramref name="spawn"/>.</summary>
        public static void GoTo(Scene current, string sceneFile, string? spawn = null)
        {
            PendingSpawn = spawn;
            PendingPosition = null;
            current.RequestSceneLoad(sceneFile);
        }

        /// <summary>Retira os objetos persistentes da cena que vai sair (chamado pelo host).</summary>
        public static void BeforeSceneSwap(Scene leaving)
        {
            _carried.Clear();
            foreach (var obj in leaving.Objects.ToList())
                if (obj.Components.OfType<Persistent>().Any())
                {
                    leaving.Remove(obj);
                    _carried.Add(obj);
                }
            Signals.ClearRuntime();
        }

        /// <summary>Insere os persistentes na cena nova e posiciona o grupo (chamado pelo host
        /// depois do StartPlay da cena nova).</summary>
        public static void AfterSceneLoaded(Scene scene)
        {
            foreach (var obj in _carried)
            {
                // Já existe um igual na cena (posto para testar a sala isolada): fica o que veio.
                var duplicate = scene.Objects.FirstOrDefault(o => o.Name == obj.Name && o.Components.OfType<Persistent>().Any());
                if (duplicate != null) scene.Remove(duplicate);
                scene.Add(obj);
            }
            _carried.Clear();

            PlaceParty(scene);
            PendingSpawn = null;
            PendingPosition = null;
            Arrived?.Invoke(scene);
        }

        /// <summary>Coloca os membros do grupo no spawn pendente (ou na posição explícita).</summary>
        public static void PlaceParty(Scene scene)
        {
            Vector2 origin;
            int facing = 1;
            float spacing = 48f;
            if (PendingPosition is { } pos)
                origin = pos;
            else
            {
                var spawns = Bodies.All(scene.Objects).SelectMany(o => o.Components.OfType<SpawnPoint>()).ToList();
                if (spawns.Count == 0 || (PendingSpawn == null && !spawns.Any(s => s.Name == "default")))
                    return;
                var spawn = spawns.FirstOrDefault(s => s.Name == (PendingSpawn ?? "default")) ?? spawns[0];
                origin = spawn.Owner.Transform.WorldPosition;
                facing = spawn.Facing >= 0 ? 1 : -1;
                spacing = spawn.PartySpacing;
            }

            var members = Bodies.All(scene.Objects)
                .Select(o => (Obj: o, P: o.Components.OfType<Persistent>().FirstOrDefault()))
                .Where(x => x.P != null && x.P.PlaceAtSpawn)
                .OrderBy(x => x.P!.PartyOrder)
                .Select(x => x.Obj)
                .ToList();
            for (int i = 0; i < members.Count; i++)
            {
                var at = origin - new Vector2(facing * spacing * i, 0f);
                var pc = members[i].Components.OfType<PlatformerController>().FirstOrDefault();
                if (pc != null) pc.Teleport(at - (members[i].Parent?.Transform.WorldPosition ?? Vector2.Zero));
                else members[i].Transform.Position = at;
            }
        }
    }

    /// <summary>Resumo de um slot de save.</summary>
    public sealed record SaveSlotInfo(int Slot, string Scene, string? Spawn, DateTime SavedAt, TimeSpan PlayTime, string Label);

    /// <summary>
    /// Slots de save: cada slot é um arquivo do <see cref="SaveGame"/> (WorldState incluído) com
    /// cena, spawn/posição, data e tempo de jogo. Pasta: <see cref="SaveGame.Folder"/>.
    /// </summary>
    public static class SaveSlots
    {
        private const string KScene = "slot.scene", KSpawn = "slot.spawn", KPosX = "slot.x", KPosY = "slot.y",
            KTime = "slot.savedAt", KPlay = "slot.playSeconds", KLabel = "slot.label";

        /// <summary>Slot em uso (o SavePoint salva nele).</summary>
        public static int CurrentSlot { get; set; } = 1;

        /// <summary>Tempo de jogo acumulado (o jogo soma a cada frame com <see cref="Tick"/>).</summary>
        public static TimeSpan PlayTime { get; set; }

        /// <summary>Texto livre gravado no slot (ex.: nome da região, nível do personagem).</summary>
        public static string Label { get; set; } = "";

        public static void Tick(float dt) => PlayTime += TimeSpan.FromSeconds(dt);

        public static string PathFor(int slot) => Path.Combine(SaveGame.Folder, $"slot{slot}.json");

        public static void Save(int slot, string sceneFile, string? spawn, Vector2? position = null)
        {
            SaveGame.SetString(KScene, sceneFile ?? "");
            if (spawn != null) SaveGame.SetString(KSpawn, spawn); else SaveGame.Delete(KSpawn);
            if (position is { } p) { SaveGame.SetFloat(KPosX, p.X); SaveGame.SetFloat(KPosY, p.Y); }
            else { SaveGame.Delete(KPosX); SaveGame.Delete(KPosY); }
            SaveGame.SetString(KTime, DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            SaveGame.SetFloat(KPlay, (float)PlayTime.TotalSeconds);
            SaveGame.SetString(KLabel, Label);
            CurrentSlot = slot;
            SaveGame.Save(PathFor(slot));
        }

        /// <summary>Carrega o slot no SaveGame/WorldState e prepara a navegação. Retorna a cena a
        /// carregar (passe para <c>SceneHost.LoadScene</c>), ou null se o slot não existe.</summary>
        public static string? Load(int slot)
        {
            if (!SaveGame.Load(PathFor(slot)))
                return null;
            CurrentSlot = slot;
            PlayTime = TimeSpan.FromSeconds(SaveGame.GetFloat(KPlay));
            Label = SaveGame.GetString(KLabel);
            WorldNavigator.PendingSpawn = SaveGame.Has(KSpawn) ? SaveGame.GetString(KSpawn) : null;
            WorldNavigator.PendingPosition = SaveGame.Has(KPosX) ? new Vector2(SaveGame.GetFloat(KPosX), SaveGame.GetFloat(KPosY)) : null;
            var scene = SaveGame.GetString(KScene);
            return string.IsNullOrEmpty(scene) ? null : scene;
        }

        public static bool Exists(int slot) => File.Exists(PathFor(slot));

        public static void Delete(int slot)
        {
            if (Exists(slot)) File.Delete(PathFor(slot));
        }

        /// <summary>Lê o resumo dos slots existentes (sem alterar o estado atual).</summary>
        public static IReadOnlyList<SaveSlotInfo> List(int maxSlots = 9)
        {
            var list = new List<SaveSlotInfo>();
            for (int i = 1; i <= maxSlots; i++)
            {
                var data = SaveGame.Peek(PathFor(i));
                if (data == null) continue;
                data.TryGetValue(KScene, out var scene);
                data.TryGetValue(KSpawn, out var spawn);
                data.TryGetValue(KTime, out var time);
                data.TryGetValue(KPlay, out var play);
                data.TryGetValue(KLabel, out var label);
                DateTime.TryParse(time, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var at);
                float.TryParse(play, NumberStyles.Float, CultureInfo.InvariantCulture, out var secs);
                list.Add(new SaveSlotInfo(i, scene ?? "", spawn, at, TimeSpan.FromSeconds(secs), label ?? ""));
            }
            return list;
        }

        /// <summary>Novo jogo: limpa o estado do mundo e o tempo.</summary>
        public static void NewGame(int slot)
        {
            SaveGame.Clear();
            CurrentSlot = slot;
            PlayTime = TimeSpan.Zero;
            Label = "";
        }
    }
}
