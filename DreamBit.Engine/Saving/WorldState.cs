using System;

namespace DreamBit.Engine.Saving
{
    /// <summary>
    /// Estado persistente do mundo (metroidvania): flags (porta aberta, parede quebrada, item
    /// coletado, chefe derrotado) e contadores. Fica dentro do <see cref="SaveGame"/> (prefixo
    /// "world.") e é salvo/carregado junto com ele.
    /// </summary>
    public static class WorldState
    {
        private const string FlagPrefix = "world.flag.";
        private const string CounterPrefix = "world.int.";

        /// <summary>Disparado quando uma flag ou contador muda (chave sem prefixo).</summary>
        public static event Action<string>? Changed;

        public static bool HasFlag(string key) => !string.IsNullOrEmpty(key) && SaveGame.GetBool(FlagPrefix + key);

        public static void SetFlag(string key, bool value = true)
        {
            if (string.IsNullOrEmpty(key) || HasFlag(key) == value) return;
            if (value) SaveGame.SetBool(FlagPrefix + key, true);
            else SaveGame.Delete(FlagPrefix + key);
            Changed?.Invoke(key);
        }

        public static int GetCounter(string key) => SaveGame.GetInt(CounterPrefix + key);

        public static void SetCounter(string key, int value)
        {
            if (string.IsNullOrEmpty(key) || GetCounter(key) == value) return;
            SaveGame.SetInt(CounterPrefix + key, value);
            Changed?.Invoke(key);
        }

        public static int AddCounter(string key, int delta = 1)
        {
            int v = GetCounter(key) + delta;
            SetCounter(key, v);
            return v;
        }

        /// <summary>Apaga todas as flags e contadores do mundo (novo jogo).</summary>
        public static void Reset()
        {
            foreach (var key in SaveGame.Snapshot().Keys)
                if (key.StartsWith(FlagPrefix, StringComparison.Ordinal) || key.StartsWith(CounterPrefix, StringComparison.Ordinal))
                    SaveGame.Delete(key);
            Changed?.Invoke(string.Empty);
        }
    }
}
