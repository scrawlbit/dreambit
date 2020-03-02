using DreamBit.Game.Elements.Components;
using DreamBit.Game.Tests.Mocks.Components;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Game.Tests.Elements.Components
{
    [TestClass]
    public class CameraObjectTest
    {
        [TestMethod]
        public void InitialActiveState()
        {
            var cameraService = new CameraServiceMock();
            var component = new CameraObject(cameraService);
            
            cameraService.CurrentCamera.Should().BeNull();
            component.Start();
            cameraService.CurrentCamera.Should().Be(component);
        }

        [TestMethod]
        public void ChangeCamera()
        {
            var cameraService = new CameraServiceMock();
            var component1 = new CameraObject(cameraService);
            var component2 = new CameraObject(cameraService);

            cameraService.CurrentCamera.Should().BeNull();

            component1.IsActive = true;
            cameraService.CurrentCamera.Should().Be(component1);

            component2.IsActive = true;
            cameraService.CurrentCamera.Should().Be(component2);
        }
        
        [TestMethod]
        public void DumpTest()
        {
            var cameraService = new CameraServiceMock();
            var component = new CameraObject(cameraService)
            {
                IsActive = true,
                Size = Vector2.One
            };

            component.IsActive = component.IsActive;
            component.Size = component.Size;
        }
    }
}