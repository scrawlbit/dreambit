using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Notification;
using Microsoft.Xna.Framework;

namespace DreamBit.Studio.ViewModels
{
    /// <summary>
    /// Adaptador editável do objeto selecionado para o painel Inspetor: expõe X/Y,
    /// rotação (em graus) e escala como propriedades bindáveis, reconstruindo os
    /// Vector2 do Transform a cada alteração (structs não são bindáveis diretamente).
    /// </summary>
    public sealed class InspectorViewModel : NotificationObject
    {
        private GameObject? _target;

        public GameObject? Target
        {
            get => _target;
            set
            {
                if (_target == value)
                    return;

                if (_target != null)
                    _target.Transform.Changed -= RaiseAll;

                _target = value;

                if (_target != null)
                    _target.Transform.Changed += RaiseAll;

                RaiseAll();
            }
        }

        public bool HasTarget => _target != null;

        public string Name
        {
            get => _target?.Name ?? string.Empty;
            set { if (_target != null) { _target.Name = value; OnPropertyChanged(); } }
        }

        public float PositionX
        {
            get => _target?.Transform.Position.X ?? 0f;
            set { if (_target != null) _target.Transform.Position = new Vector2(value, _target.Transform.Position.Y); }
        }

        public float PositionY
        {
            get => _target?.Transform.Position.Y ?? 0f;
            set { if (_target != null) _target.Transform.Position = new Vector2(_target.Transform.Position.X, value); }
        }

        public float RotationDegrees
        {
            get => _target != null ? MathHelper.ToDegrees(_target.Transform.Rotation) : 0f;
            set { if (_target != null) _target.Transform.Rotation = MathHelper.ToRadians(value); }
        }

        public float ScaleX
        {
            get => _target?.Transform.Scale.X ?? 1f;
            set { if (_target != null) _target.Transform.Scale = new Vector2(value, _target.Transform.Scale.Y); }
        }

        public float ScaleY
        {
            get => _target?.Transform.Scale.Y ?? 1f;
            set { if (_target != null) _target.Transform.Scale = new Vector2(_target.Transform.Scale.X, value); }
        }

        // ---- Componente SpriteRenderer ----

        private SpriteRenderer? Sprite => _target?.Components.OfType<SpriteRenderer>().FirstOrDefault();
        public bool HasSprite => Sprite != null;

        public float SpriteWidth
        {
            get => Sprite?.Size.X ?? 0f;
            set { var s = Sprite; if (s != null) s.Size = new Vector2(value, s.Size.Y); }
        }
        public float SpriteHeight
        {
            get => Sprite?.Size.Y ?? 0f;
            set { var s = Sprite; if (s != null) s.Size = new Vector2(s.Size.X, value); }
        }
        public int SpriteR
        {
            get => Sprite?.Color.R ?? 0;
            set { var s = Sprite; if (s != null) s.Color = new Color((byte)Clamp(value), s.Color.G, s.Color.B); }
        }
        public int SpriteG
        {
            get => Sprite?.Color.G ?? 0;
            set { var s = Sprite; if (s != null) s.Color = new Color(s.Color.R, (byte)Clamp(value), s.Color.B); }
        }
        public int SpriteB
        {
            get => Sprite?.Color.B ?? 0;
            set { var s = Sprite; if (s != null) s.Color = new Color(s.Color.R, s.Color.G, (byte)Clamp(value)); }
        }

        // ---- Componente RotatorBehavior ----

        private RotatorBehavior? Rotator => _target?.Components.OfType<RotatorBehavior>().FirstOrDefault();
        public bool HasRotator => Rotator != null;

        public float RotatorSpeed
        {
            get => Rotator?.Speed ?? 0f;
            set { var r = Rotator; if (r != null) r.Speed = value; }
        }

        /// <summary>Reavalia todos os campos (ex.: após adicionar/remover componente).</summary>
        public void Refresh() => RaiseAll();

        private static int Clamp(int value) => value < 0 ? 0 : value > 255 ? 255 : value;

        private void RaiseAll()
        {
            OnPropertyChanged(nameof(HasTarget));
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(PositionX));
            OnPropertyChanged(nameof(PositionY));
            OnPropertyChanged(nameof(RotationDegrees));
            OnPropertyChanged(nameof(ScaleX));
            OnPropertyChanged(nameof(ScaleY));
            OnPropertyChanged(nameof(HasSprite));
            OnPropertyChanged(nameof(SpriteWidth));
            OnPropertyChanged(nameof(SpriteHeight));
            OnPropertyChanged(nameof(SpriteR));
            OnPropertyChanged(nameof(SpriteG));
            OnPropertyChanged(nameof(SpriteB));
            OnPropertyChanged(nameof(HasRotator));
            OnPropertyChanged(nameof(RotatorSpeed));
        }
    }
}
