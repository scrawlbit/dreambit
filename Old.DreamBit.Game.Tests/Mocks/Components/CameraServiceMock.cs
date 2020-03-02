using DreamBit.Game.Components;

namespace DreamBit.Game.Tests.Mocks.Components
{
    internal class CameraServiceMock : ICameraService
    {
        public ICamera CurrentCamera { get; set; }

        public void UpdateSceneCamera()
        {   
        }
    }
}