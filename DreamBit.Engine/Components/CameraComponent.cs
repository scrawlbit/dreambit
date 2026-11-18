using System;
using System.Collections.Generic;
using System.Linq;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Messaging;
using DreamBit.Engine.Rendering;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Câmera de cena configurável: segue um alvo (por tag, ex.: "Player") com deadzone,
    /// suavização, zoom, olhar à frente e limites (bounds). Com várias tags (ex.: "Ruber,Aloine")
    /// ou várias instâncias da tag, enquadra o grupo: centraliza entre os alvos e dá zoom out
    /// até <see cref="MinZoom"/> para manter todos na tela (co-op sem tela dividida).
    /// O host (Player/editor no play) chama <see cref="DriveCamera"/> por frame.
    /// </summary>
    public sealed class CameraComponent : SceneComponent, IMessageReceiver
    {
        private string _targetTag = "Player";
        private float _deadzoneWidth = 120f;
        private float _deadzoneHeight = 80f;
        private float _smoothTime = 0.15f;
        private float _zoom = 1f;
        private bool _useBounds;
        private Vector2 _boundsMin = new(-2000, -2000);
        private Vector2 _boundsMax = new(2000, 2000);
        private int _priority;
        private float _minZoom = 0.6f;
        private Vector2 _groupPadding = new(240f, 180f);
        private float _zoomSmoothTime = 0.35f;
        private float _lookAhead;
        private float _lookAheadSmoothTime = 0.4f;

        private float _currentZoom = -1f;
        private float _lookAheadOffset;

        // Shake (tremor) — amplitude decai ao longo da duração.
        private static readonly Random Rng = new();
        private float _shakeTime;
        private float _shakeDuration;
        private float _shakeMagnitude;
        private string _shakeOnMessage = string.Empty;
        private float _shakeMsgDuration = 0.3f;
        private float _shakeMsgMagnitude = 12f;

        public override string DisplayName => "Camera";

        /// <summary>Prioridade: o host usa a câmera ativa de maior prioridade (troca de câmera).</summary>
        public int Priority { get => _priority; set => Set(ref _priority, value); }

        /// <summary>Mensagem que dispara um tremor (ex.: "hit", "explosao"). Vazio = nenhuma.</summary>
        public string ShakeOnMessage { get => _shakeOnMessage; set => Set(ref _shakeOnMessage, value ?? string.Empty); }
        public float ShakeMessageDuration { get => _shakeMsgDuration; set => Set(ref _shakeMsgDuration, Math.Max(0f, value)); }
        public float ShakeMessageMagnitude { get => _shakeMsgMagnitude; set => Set(ref _shakeMsgMagnitude, Math.Max(0f, value)); }

        /// <summary>Inicia um tremor de câmera por <paramref name="duration"/> s com amplitude
        /// <paramref name="magnitude"/> px (decai até zero). Chame de scripts/combate.</summary>
        public void Shake(float duration, float magnitude)
        {
            if (duration <= 0f || magnitude <= 0f)
                return;
            _shakeDuration = duration;
            _shakeTime = duration;
            _shakeMagnitude = magnitude;
        }

        void IMessageReceiver.OnMessage(GameMessage message)
        {
            if (!string.IsNullOrEmpty(_shakeOnMessage) && message.Name == _shakeOnMessage)
                Shake(_shakeMsgDuration, _shakeMsgMagnitude);
        }

        /// <summary>Tag(s) do(s) alvo(s), separadas por vírgula (vazio = segue o próprio objeto).
        /// Todos os objetos com essas tags formam o grupo enquadrado.</summary>
        public string TargetTag { get => _targetTag; set => Set(ref _targetTag, value ?? string.Empty); }

        /// <summary>Retângulo em que o alvo se move sem mover a câmera.</summary>
        public float DeadzoneWidth { get => _deadzoneWidth; set => Set(ref _deadzoneWidth, Math.Max(0f, value)); }
        public float DeadzoneHeight { get => _deadzoneHeight; set => Set(ref _deadzoneHeight, Math.Max(0f, value)); }

        /// <summary>Tempo de suavização (s); 0 = gruda no alvo.</summary>
        public float SmoothTime { get => _smoothTime; set => Set(ref _smoothTime, Math.Max(0f, value)); }

        /// <summary>Zoom normal (mais próximo) da câmera.</summary>
        public float Zoom { get => _zoom; set => Set(ref _zoom, Math.Max(0.05f, value)); }

        /// <summary>Zoom mínimo (o mais afastado) ao enquadrar um grupo.</summary>
        public float MinZoom { get => _minZoom; set => Set(ref _minZoom, Math.Max(0.05f, value)); }

        /// <summary>Margem (px de mundo, por lado) mantida em volta do grupo.</summary>
        public Vector2 GroupPadding { get => _groupPadding; set => Set(ref _groupPadding, Vector2.Max(Vector2.Zero, value)); }

        /// <summary>Suavização da troca de zoom (s).</summary>
        public float ZoomSmoothTime { get => _zoomSmoothTime; set => Set(ref _zoomSmoothTime, Math.Max(0f, value)); }

        /// <summary>Quanto (px) a câmera olha à frente na direção do movimento horizontal do alvo.</summary>
        public float LookAhead { get => _lookAhead; set => Set(ref _lookAhead, Math.Max(0f, value)); }
        public float LookAheadSmoothTime { get => _lookAheadSmoothTime; set => Set(ref _lookAheadSmoothTime, Math.Max(0f, value)); }

        public bool UseBounds { get => _useBounds; set => Set(ref _useBounds, value); }
        public Vector2 BoundsMin { get => _boundsMin; set => Set(ref _boundsMin, value); }
        public Vector2 BoundsMax { get => _boundsMax; set => Set(ref _boundsMax, value); }

        /// <summary>Limites temporários (ex.: zona/sala atual), com prioridade sobre os fixos. Não é salvo.</summary>
        public (Vector2 Min, Vector2 Max)? BoundsOverride { get; set; }

        /// <summary>Zoom efetivo aplicado no último frame.</summary>
        public float CurrentZoom => _currentZoom > 0f ? _currentZoom : _zoom;

        /// <summary>True quando o grupo não cabe nem no <see cref="MinZoom"/> (o jogo pode puxar
        /// o parceiro de volta, teleportar a IA etc.).</summary>
        public bool GroupOverflow { get; private set; }

        /// <summary>Alvos encontrados no último frame.</summary>
        public IReadOnlyList<GameObject> Targets => _targets;
        private readonly List<GameObject> _targets = new();

        /// <summary>Atualiza a câmera para seguir o(s) alvo(s) com deadzone/suavização/bounds.</summary>
        public void DriveCamera(Camera2D camera, float dt, int viewportWidth, int viewportHeight)
        {
            FindTargets();
            if (_targets.Count == 0)
            {
                camera.Zoom = CurrentZoom;
                return;
            }

            var (focus, desiredZoom) = Frame(viewportWidth, viewportHeight);

            // Zoom suave.
            if (_currentZoom <= 0f) _currentZoom = desiredZoom;
            float zt = _zoomSmoothTime <= 0f ? 1f : Scrawlbit.Mathf.Clamp01(dt / _zoomSmoothTime);
            _currentZoom = MathHelper.Lerp(_currentZoom, desiredZoom, zt);
            camera.Zoom = _currentZoom;

            // Olhar à frente (só com um alvo).
            if (_lookAhead > 0f && _targets.Count == 1)
            {
                float dir = Math.Sign(TargetVelocityX(_targets[0]));
                float lt = _lookAheadSmoothTime <= 0f ? 1f : Scrawlbit.Mathf.Clamp01(dt / _lookAheadSmoothTime);
                if (dir != 0f)
                    _lookAheadOffset = MathHelper.Lerp(_lookAheadOffset, dir * _lookAhead, lt);
                focus.X += _lookAheadOffset;
            }

            var current = camera.Position;

            // Deadzone: só move quando o foco sai do retângulo (em grupo, segue o centro direto).
            var desired = current;
            if (_targets.Count > 1)
                desired = focus;
            else
            {
                float dzx = _deadzoneWidth / 2f, dzy = _deadzoneHeight / 2f;
                if (focus.X > current.X + dzx) desired.X = focus.X - dzx;
                else if (focus.X < current.X - dzx) desired.X = focus.X + dzx;
                if (focus.Y > current.Y + dzy) desired.Y = focus.Y - dzy;
                else if (focus.Y < current.Y - dzy) desired.Y = focus.Y + dzy;
            }

            // Suavização + limites.
            float t = _smoothTime <= 0f ? 1f : Scrawlbit.Mathf.Clamp01(dt / _smoothTime);
            var next = ClampToBounds(Vector2.Lerp(current, desired, t), viewportWidth, viewportHeight, _currentZoom);

            // Tremor: offset aleatório com amplitude decaindo.
            if (_shakeTime > 0f && _shakeDuration > 0f)
            {
                _shakeTime = Math.Max(0f, _shakeTime - dt);
                float amp = _shakeMagnitude * (_shakeTime / _shakeDuration);
                next += new Vector2(
                    ((float)Rng.NextDouble() * 2f - 1f) * amp,
                    ((float)Rng.NextDouble() * 2f - 1f) * amp);
            }

            camera.Position = next;
        }

        /// <summary>Coloca a câmera direto no enquadramento, sem deadzone/suavização (início de
        /// cena, teleporte, troca de sala).</summary>
        public void SnapCamera(Camera2D camera, int viewportWidth, int viewportHeight)
        {
            FindTargets();
            if (_targets.Count == 0)
            {
                camera.Zoom = _zoom;
                return;
            }
            var (focus, zoom) = Frame(viewportWidth, viewportHeight);
            _currentZoom = zoom;
            _lookAheadOffset = 0f;
            camera.Zoom = zoom;
            camera.Position = ClampToBounds(focus, viewportWidth, viewportHeight, zoom);
        }

        /// <summary>Centro e zoom que enquadram os alvos (zoom em [MinZoom, Zoom]).</summary>
        private (Vector2 Focus, float Zoom) Frame(int viewportWidth, int viewportHeight)
        {
            if (_targets.Count == 1)
            {
                GroupOverflow = false;
                return (_targets[0].Transform.WorldPosition, _zoom);
            }

            var min = new Vector2(float.MaxValue);
            var max = new Vector2(float.MinValue);
            foreach (var t in _targets)
            {
                var p = t.Transform.WorldPosition;
                min = Vector2.Min(min, p);
                max = Vector2.Max(max, p);
            }
            var size = max - min + _groupPadding * 2f;
            float fit = Math.Min(viewportWidth / Math.Max(1f, size.X), viewportHeight / Math.Max(1f, size.Y));
            float floor = Math.Min(_minZoom, _zoom);
            GroupOverflow = fit < floor;
            return ((min + max) / 2f, MathHelper.Clamp(fit, floor, _zoom));
        }

        private Vector2 ClampToBounds(Vector2 p, int viewportWidth, int viewportHeight, float zoom)
        {
            (Vector2 Min, Vector2 Max)? bounds = BoundsOverride ?? (_useBounds ? (_boundsMin, _boundsMax) : null);
            if (bounds is not { } b)
                return p;
            float halfW = viewportWidth / 2f / zoom;
            float halfH = viewportHeight / 2f / zoom;
            p.X = ClampAxis(p.X, b.Min.X + halfW, b.Max.X - halfW);
            p.Y = ClampAxis(p.Y, b.Min.Y + halfH, b.Max.Y - halfH);
            return p;
        }

        /// <summary>True enquanto um tremor está em andamento.</summary>
        public bool IsShaking => _shakeTime > 0f;

        private static float ClampAxis(float value, float min, float max)
            => min > max ? (min + max) / 2f : Scrawlbit.Mathf.Clamp(value, min, max);

        private static float TargetVelocityX(GameObject target)
        {
            foreach (var c in target.Components)
                if (c is PlatformerController pc) return pc.CurrentVelocityX;
            return 0f;
        }

        private void FindTargets()
        {
            _targets.Clear();
            var scene = Owner.Scene;
            if (string.IsNullOrWhiteSpace(_targetTag) || scene == null)
            {
                _targets.Add(Owner);
                return;
            }

            var tags = _targetTag.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var obj in All(scene.Objects))
                if (tags.Any(obj.HasTag))
                    _targets.Add(obj);

            // Compatibilidade: "Player" também acha o primeiro objeto com PlatformerController.
            if (_targets.Count == 0 && tags.Any(t => string.Equals(t, "Player", StringComparison.OrdinalIgnoreCase)))
            {
                var p = All(scene.Objects).FirstOrDefault(o => o.Components.OfType<PlatformerController>().Any());
                if (p != null) _targets.Add(p);
            }

            if (_targets.Count == 0)
                _targets.Add(Owner);
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
