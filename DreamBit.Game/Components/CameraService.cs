namespace DreamBit.Game.Components
{
    internal class CameraService : ICameraService
    {
        private readonly ISceneCamera _sceneCamera;
        private ICamera _currentCamera;

        public CameraService(ISceneCamera sceneCamera)
        {
            _sceneCamera = sceneCamera;
        }

        public ICamera CurrentCamera
        {
            get => _currentCamera;
            set
            {
                if (value == _currentCamera) return;

                if (_currentCamera != null)
                    _currentCamera.IsActive = false;

                _currentCamera = value;
                _currentCamera.IsActive = true;
            }
        }

        public void UpdateSceneCamera()
        {
            if (CurrentCamera?.IsActive == true)
            {
                _sceneCamera.Size = CurrentCamera.Size;
                _sceneCamera.Position = CurrentCamera.Position;
                _sceneCamera.Rotation = CurrentCamera.Rotation;
                _sceneCamera.Zoom = CurrentCamera.Zoom;
            }
        }
    }
}