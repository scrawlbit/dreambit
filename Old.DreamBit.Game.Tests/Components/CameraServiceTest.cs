using DreamBit.Game.Components;
using DreamBit.Game.Elements.Components;
using DreamBit.Game.Tests.Mocks.Components;
using DreamBit.Game.Tests.Mocks.Elements;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Scrawlbit.MonoGame.Tests;

namespace DreamBit.Game.Tests.Components
{
    [TestClass]
    public class CameraServiceTest
    {
        [TestMethod]
        public void InitialValues()
        {
            var sceneCamera = new SceneCameraMock();
            var service = new CameraService(sceneCamera);
            
            service.CurrentCamera.Should().BeNull();
        }

        [TestMethod]
        public void ChangeCamera()
        {
            var sceneCamera = new SceneCameraMock();
            var service = new CameraService(sceneCamera);
            var camera1 = new CameraMock();
            var camera2 = new CameraMock();

            service.CurrentCamera = null;
            service.CurrentCamera.Should().BeNull();

            service.CurrentCamera = camera1;
            service.CurrentCamera.Should().Be(camera1);
            camera1.IsActive.Should().BeTrue();

            service.CurrentCamera = camera2;
            service.CurrentCamera.Should().Be(camera2);
            camera2.IsActive.Should().BeTrue();
            camera1.IsActive.Should().BeFalse();
        }
        [TestMethod]
        public void UpdateCameraValues()
        {
            var sceneCamera = new SceneCameraMock();
            var cameraService = new CameraService(sceneCamera);
            var gameObject = new GameObjectMock
            {
                Transform =
                {
                    Position = new Vector2(100),
                    Rotation = MathHelper.Pi,
                    Scale = new Vector2(1.5f)
                }
            };
            var component = new CameraObject(cameraService)
            {
                Size = new Vector2(800, 600),
                IsActive = true
            };

            component.Initialize(gameObject);
            cameraService.UpdateSceneCamera();

            sceneCamera.Size.Should().Be(new Vector2(800, 600));
            sceneCamera.Position.Should().Be(new Vector2(100));
            sceneCamera.Rotation.Should().Be(MathHelper.Pi);
            sceneCamera.Zoom.Should().Be(new Vector2(1.5f));
        }

        [TestMethod]
        public void UpdateMultipleComponents()
        {
            var sceneCamera = new SceneCameraMock();
            var cameraService = new CameraService(sceneCamera);
            var gameObject1 = new GameObjectMock { Transform = { Position = new Vector2(100) } };
            var gameObject2 = new GameObjectMock { Transform = { Position = new Vector2(200) } };
            var component1 = new CameraObject(cameraService) { IsActive = true };
            var component2 = new CameraObject(cameraService);

            component1.Initialize(gameObject1);
            component2.Initialize(gameObject2);
            cameraService.UpdateSceneCamera();

            sceneCamera.Position.Should().Be(new Vector2(100));

            component2.IsActive = true;
            cameraService.UpdateSceneCamera();

            sceneCamera.Position.Should().Be(new Vector2(200));

            sceneCamera.Position = new Vector2(300);
            component2.IsActive = false;
            cameraService.UpdateSceneCamera();

            cameraService.CurrentCamera.IsActive.Should().BeFalse();
            sceneCamera.Position.Should().Be(new Vector2(300));
        }
    }
}