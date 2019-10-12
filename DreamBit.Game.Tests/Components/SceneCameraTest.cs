using DreamBit.Game.Components;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Scrawlbit.MonoGame.Helpers;
using Scrawlbit.MonoGame.Tests;

namespace DreamBit.Game.Tests.Components
{
    [TestClass]
    public class SceneCameraTest
    {
        [TestMethod]
        public void InitialValues()
        {
            var camera = new SceneCamera();

            camera.Position.Should().Be(Vector2.Zero);
            camera.Size.Should().Be(Vector2.Zero);
            camera.Rotation.Should().Be(0);
            camera.Zoom.Should().Be(Vector2.One);

            camera.TransformMatrix.Decompose(out Vector2 transformPosition, out float transformRotation, out Vector2 transformScale);

            transformPosition.Should().Be(Vector2.Zero);
            transformRotation.Should().Be(0);
            transformScale.Should().Be(Vector2.One);
        }

        [TestMethod]
        public void RandomValues()
        {
            var camera = new SceneCamera
            {
                Size = new Vector2(800, 600),
                Position = new Vector2(100, 100),
                Rotation = MathHelper.PiOver4,
                Zoom = Vector2.Zero
            };

            camera.TransformMatrix.Decompose(out Vector2 transformPosition, out float transformRotation, out Vector2 transformScale);

            transformPosition.Should().Be(new Vector2(300, 200));
            transformRotation.Should().BeApproximately(MathHelper.PiOver4);
            transformScale.Should().Be(new Vector2(0.1f));
        }

        [TestMethod]
        public void ZoomInOut()
        {
            var camera = new SceneCamera();

            camera.ZoomIn(0.2f);
            camera.Zoom.Should().Be(new Vector2(1.2f));

            camera.ZoomOut(0.4f);
            camera.Zoom.Should().BeApproximately(new Vector2(0.8f));

            camera.ZoomOut(0.8f);
            camera.Zoom.Should().Be(new Vector2(0.1f));
        }
    }
}