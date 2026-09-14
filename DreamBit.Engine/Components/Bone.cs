using System;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Osso de um rig 2D estilo cutout: marca o GameObject como um osso do esqueleto.
    /// Não deforma malha — o rig é a própria hierarquia de Transform (pai→filho), e as
    /// partes (sprites) são objetos filhos dos ossos. Guarda uma pose de descanso para
    /// poder voltar o rig ao repouso. A animação por keyframes fica para um passo futuro.
    /// </summary>
    public sealed class Bone : SceneComponent
    {
        private float _length = 40f;
        private Vector2 _restPosition;
        private float _restRotation;
        private Vector2 _restScale = Vector2.One;
        private bool _hasRestPose;

        public override string DisplayName => "Bone";

        /// <summary>Comprimento do osso (para visualização e para posicionar a ponta/filhos).</summary>
        public float Length
        {
            get => _length;
            set => Set(ref _length, Math.Max(0f, value));
        }

        // Pose de descanso (transform local). Públicos para serialização.
        public Vector2 RestPosition { get => _restPosition; set => Set(ref _restPosition, value); }
        public float RestRotation { get => _restRotation; set => Set(ref _restRotation, value); }
        public Vector2 RestScale { get => _restScale; set => Set(ref _restScale, value); }
        public bool HasRestPose { get => _hasRestPose; set => Set(ref _hasRestPose, value); }

        /// <summary>Ponta do osso em coordenadas de mundo (origem + comprimento no eixo X local).</summary>
        public Vector2 WorldTip => Vector2.Transform(new Vector2(_length, 0f), Owner.Transform.WorldMatrix);

        /// <summary>Captura a pose atual como pose de descanso.</summary>
        public void CaptureRestPose()
        {
            var t = Owner.Transform;
            _restPosition = t.Position;
            _restRotation = t.Rotation;
            _restScale = t.Scale;
            HasRestPose = true;
        }

        /// <summary>Volta o transform do osso à pose de descanso (se capturada).</summary>
        public void ResetToRestPose()
        {
            if (!_hasRestPose)
                return;

            var t = Owner.Transform;
            t.Position = _restPosition;
            t.Rotation = _restRotation;
            t.Scale = _restScale;
        }

        protected internal override void OnPlayStarted()
        {
            if (!_hasRestPose)
                CaptureRestPose();
        }

        protected internal override void Draw(ISceneDrawing drawing)
        {
            var world = Owner.Transform.WorldMatrix;

            // Corpo do osso: retângulo fino do joint até a ponta (centro no meio-comprimento local).
            if (_length > 0.01f)
            {
                var body = Matrix.CreateTranslation(_length / 2f, 0f, 0f) * world;
                drawing.DrawQuad(body, new Vector2(_length, 3f), new Color(240, 200, 90) * 0.7f);
            }

            // Joint: marcador no ponto de articulação.
            drawing.DrawQuad(world, new Vector2(8f, 8f), new Color(255, 230, 140));
        }
    }
}
