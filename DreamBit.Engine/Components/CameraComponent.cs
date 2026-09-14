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
    /// suavização, zoom e limites (bounds).
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

        /// <summary>Tag do alvo a seguir (vazio = segue o próprio objeto).</summary>
        public string TargetTag { get => _targetTag; set => Set(ref _targetTag, value ?? string.Empty); }

        /// <summary>Retângulo em que o alvo se move sem mover a câmera.</summary>
        public float DeadzoneWidth { get => _deadzoneWidth; set => Set(ref _deadzoneWidth, Math.Max(0f, value)); }
        public float DeadzoneHeight { get => _deadzoneHeight; set => Set(ref _deadzoneHeight, Math.Max(0f, value)); }

        /// <summary>Tempo de suavização (s); 0 = gruda no alvo.</summary>
        public float SmoothTime { get => _smoothTime; set => Set(ref _smoothTime, Math.Max(0f, value)); }

        public float Zoom { get => _zoom; set => Set(ref _zoom, Math.Max(0.05f, value)); }

        public bool UseBounds { get => _useBounds; set => Set(ref _useBounds, value); }
        public Vector2 BoundsMin { get => _boundsMin; set => Set(ref _boundsMin, value); }
        public Vector2 BoundsMax { get => _boundsMax; set => Set(ref _boundsMax, value); }

        /// <summary>Atualiza a câmera para seguir o alvo com deadzone/suavização/bounds.</summary>
        public void DriveCamera(Camera2D camera, float dt, int viewportWidth, int viewportHeight)
        {
            camera.Zoom = _zoom;

            var target = FindTarget();
            if (target == null)
                return;

            var focus = target.Transform.WorldPosition;
            var current = camera.Position;

            // Deadzone: só move a câmera quando o alvo sai do retângulo.
            float dzx = _deadzoneWidth / 2f, dzy = _deadzoneHeight / 2f;
            var desired = current;
            if (focus.X > current.X + dzx) desired.X = focus.X - dzx;
            else if (focus.X < current.X - dzx) desired.X = focus.X + dzx;
            if (focus.Y > current.Y + dzy) desired.Y = focus.Y - dzy;
            else if (focus.Y < current.Y - dzy) desired.Y = focus.Y + dzy;

            // Suavização.
            float t = _smoothTime <= 0f ? 1f : Scrawlbit.Mathf.Clamp01(dt / _smoothTime);
            var next = Vector2.Lerp(current, desired, t);

            // Limites: mantém a vista dentro do retângulo (quando cabe).
            if (_useBounds)
            {
                float halfW = viewportWidth / 2f / _zoom;
                float halfH = viewportHeight / 2f / _zoom;
                next.X = ClampAxis(next.X, _boundsMin.X + halfW, _boundsMax.X - halfW);
                next.Y = ClampAxis(next.Y, _boundsMin.Y + halfH, _boundsMax.Y - halfH);
            }

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

        /// <summary>True enquanto um tremor está em andamento.</summary>
        public bool IsShaking => _shakeTime > 0f;

        private static float ClampAxis(float value, float min, float max)
            => min > max ? (min + max) / 2f : Scrawlbit.Mathf.Clamp(value, min, max);

        private GameObject? FindTarget()
        {
            if (string.IsNullOrEmpty(_targetTag))
                return Owner;

            var scene = Owner.Scene;
            if (scene == null)
                return Owner;

            foreach (var obj in All(scene.Objects))
            {
                if (obj.HasTag(_targetTag))
                    return obj;
                if (string.Equals(_targetTag, "Player", StringComparison.OrdinalIgnoreCase) &&
                    obj.Components.OfType<PlatformerController>().Any())
                    return obj;
            }
            return Owner;
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
