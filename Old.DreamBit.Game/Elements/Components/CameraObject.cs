using DreamBit.Game.Components;
using Microsoft.Xna.Framework;

namespace DreamBit.Game.Elements.Components
{
    internal sealed class CameraObject : GameObjectComponent, ICamera
    {
        private readonly ICameraService _cameraService;
        private bool _isActive;

        internal CameraObject(ICameraService cameraService)
        {
            _cameraService = cameraService;
        }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (value == _isActive) return;

                _isActive = value;

                if (_isActive)
                    _cameraService.CurrentCamera = this;
            }
        }
        public Vector2 Size { get; set; }
        public Vector2 Position => GameObject.Transform.Position;
        public float Rotation => GameObject.Transform.Rotation;
        public Vector2 Zoom => GameObject.Transform.Scale;

        protected internal override void Start()
        {
            if (IsActive || _cameraService.CurrentCamera == null)
                _cameraService.CurrentCamera = this;
        }
    }
}