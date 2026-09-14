using System.Collections.Generic;
using DreamBit.Engine.Audio;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Messaging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Uma camada (stem) da música: um .wav em loop que toca junto com a base, mas cujo
    /// volume sobe/desce conforme uma condição. Ativa quando há inimigos na tela
    /// (<see cref="Tag"/> + <see cref="MinCount"/>) ou por evento (<see cref="RiseMessage"/>/
    /// <see cref="FallMessage"/>).
    /// </summary>
    public sealed class MusicLayer
    {
        /// <summary>Arquivo .wav do stem (mesmo comprimento da base, para ficar em compasso).</summary>
        public string? TrackPath { get; set; }

        /// <summary>Tag que ativa a camada (ex.: "Inimigo"). Vazio = camada por evento/mensagem.</summary>
        public string Tag { get; set; } = string.Empty;

        /// <summary>Quantos objetos com a tag precisam estar presentes para ativar (padrão 1).</summary>
        public int MinCount { get; set; } = 1;

        /// <summary>Se conta só quem está no quadro da câmera (true) ou toda a cena (false).</summary>
        public bool OnlyOnScreen { get; set; } = true;

        /// <summary>Mensagem da cena que liga a camada (camada por evento). Vazio = ignora.</summary>
        public string RiseMessage { get; set; } = string.Empty;

        /// <summary>Mensagem da cena que desliga a camada (camada por evento). Vazio = ignora.</summary>
        public string FallMessage { get; set; } = string.Empty;

        /// <summary>Tempo do fade de entrada/saída, em segundos.</summary>
        public float FadeTime { get; set; } = 1.5f;

        /// <summary>Volume máximo desta camada quando ativa (0..1).</summary>
        public float MaxVolume { get; set; } = 1f;

        // ---- estado de runtime ----
        internal SoundEffectInstance? Instance;
        internal bool MessageActive;

        /// <summary>Volume atual do stem [0..1] (para depurar/visualizar a intensidade).</summary>
        public float Current { get; internal set; }
    }

    /// <summary>
    /// Música adaptativa em camadas: uma faixa base sempre tocando e camadas (stems) que
    /// entram e saem no fade conforme o que acontece no jogo — ex.: a bateria sobe quando há
    /// inimigos na tela. Todos os stems iniciam juntos e ficam em loop, então permanecem em
    /// sincronia; só o volume de cada camada muda. Configure no editor ou por código.
    /// </summary>
    public sealed class LayeredMusic : SceneComponent, IMessageReceiver
    {
        private string? _baseTrackPath;
        private float _baseVolume = 1f;
        private string _bus = AudioMixer.Music;

        private SoundEffectInstance? _baseInstance;
        private readonly List<MusicLayer> _layers = new();

        public override string DisplayName => "Layered Music";

        /// <summary>Faixa base (.wav em loop) que toca o tempo todo.</summary>
        public string? BaseTrackPath { get => _baseTrackPath; set => Set(ref _baseTrackPath, value); }

        /// <summary>Volume da faixa base (0..1).</summary>
        public float BaseVolume { get => _baseVolume; set => Set(ref _baseVolume, MathHelper.Clamp(value, 0f, 1f)); }

        /// <summary>Barramento do mixer (padrão "Music").</summary>
        public string Bus { get => _bus; set => Set(ref _bus, string.IsNullOrEmpty(value) ? AudioMixer.Music : value); }

        /// <summary>Camadas (stems) configuradas.</summary>
        public IReadOnlyList<MusicLayer> Layers => _layers;

        public MusicLayer AddLayer(MusicLayer layer)
        {
            _layers.Add(layer);
            return layer;
        }

        public bool RemoveLayer(MusicLayer layer)
        {
            Silence(layer?.Instance);
            return layer != null && _layers.Remove(layer);
        }

        public void ClearLayers()
        {
            foreach (var l in _layers)
                Silence(l.Instance);
            _layers.Clear();
        }

        protected internal override void OnPlayStarted()
        {
            float effective = AudioMixer.Effective(_bus);

            _baseInstance = Start(_baseTrackPath);
            if (_baseInstance != null)
                _baseInstance.Volume = MathHelper.Clamp(_baseVolume, 0f, 1f) * effective;

            foreach (var layer in _layers)
            {
                layer.Current = 0f;
                layer.MessageActive = false;
                layer.Instance = Start(layer.TrackPath);
                if (layer.Instance != null)
                    layer.Instance.Volume = 0f; // começa silenciosa, entra no fade
            }
        }

        protected internal override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float effective = AudioMixer.Effective(_bus);
            var scene = Owner?.Scene;

            // Base reflete ao vivo o volume do bus/Master (ex.: menu de opções).
            Apply(_baseInstance, MathHelper.Clamp(_baseVolume, 0f, 1f), effective);

            foreach (var layer in _layers)
            {
                bool active = IsActive(layer, scene);
                float target = active ? MathHelper.Clamp(layer.MaxVolume, 0f, 1f) : 0f;

                // Fade linear até o alvo.
                if (layer.FadeTime <= 0f)
                {
                    layer.Current = target;
                }
                else
                {
                    float step = dt / layer.FadeTime;
                    if (layer.Current < target)
                        layer.Current = System.Math.Min(target, layer.Current + step);
                    else if (layer.Current > target)
                        layer.Current = System.Math.Max(target, layer.Current - step);
                }

                Apply(layer.Instance, layer.Current, effective);
            }
        }

        private bool IsActive(MusicLayer layer, Scene? scene)
        {
            if (!string.IsNullOrEmpty(layer.Tag))
            {
                if (scene == null)
                    return false;
                int count = layer.OnlyOnScreen
                    ? CameraVision.CountOnScreen(scene, layer.Tag)
                    : CountTag(scene, layer.Tag);
                return count >= System.Math.Max(1, layer.MinCount);
            }
            // Camada por evento: ligada/desligada por mensagens.
            return layer.MessageActive;
        }

        void IMessageReceiver.OnMessage(GameMessage message)
        {
            foreach (var layer in _layers)
            {
                if (!string.IsNullOrEmpty(layer.RiseMessage) && message.Name == layer.RiseMessage)
                    layer.MessageActive = true;
                if (!string.IsNullOrEmpty(layer.FallMessage) && message.Name == layer.FallMessage)
                    layer.MessageActive = false;
            }
        }

        private static int CountTag(Scene scene, string tag)
        {
            int total = 0;
            foreach (var obj in All(scene.Objects))
                if (obj.HasTag(tag))
                    total++;
            return total;
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

        private static SoundEffectInstance? Start(string? path)
        {
            var sound = SoundCache.Get(path);
            if (sound == null)
                return null;
            try
            {
                var instance = sound.CreateInstance();
                instance.IsLooped = true;
                instance.Volume = 0f;
                instance.Play();
                return instance;
            }
            catch
            {
                return null; // motor de áudio indisponível (ex.: editor sem device)
            }
        }

        private static void Apply(SoundEffectInstance? instance, float level, float effective)
        {
            if (instance == null)
                return;
            try { instance.Volume = MathHelper.Clamp(level, 0f, 1f) * effective; }
            catch { /* device indisponível */ }
        }

        private static void Silence(SoundEffectInstance? instance)
        {
            try { instance?.Stop(); }
            catch { /* ignora */ }
        }
    }
}
