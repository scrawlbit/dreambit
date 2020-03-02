using DreamBit.Game.Elements;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Scrawlbit.MonoGame.Tests;

namespace DreamBit.Game.Tests.Elements
{
    [TestClass]
    public class TransformTest
    {
        [TestMethod]
        public void InitialTransformValues()
        {
            var transform = new Transform();

            transform.RelativePosition.Should().Be(Vector2.Zero);
            transform.RelativeRotation.Should().Be(0);
            transform.RelativeScale.Should().Be(Vector2.One);

            transform.Position.Should().Be(Vector2.Zero);
            transform.Rotation.Should().Be(0);
            transform.Scale.Should().Be(Vector2.One);
        }

        [TestMethod]
        public void SetRelativeValues()
        {
            var transform = new Transform
            {
                RelativePosition = new Vector2(100, 200),
                RelativeRotation = MathHelper.PiOver4,
                RelativeScale = new Vector2(2, 3)
            };

            transform.Position.Should().Be(new Vector2(100, 200));
            transform.Rotation.Should().BeApproximately(MathHelper.PiOver4);
            transform.Scale.Should().BeApproximately(new Vector2(2, 3));
        }

        [TestMethod]
        public void SetRealValues()
        {
            var transform = new Transform
            {
                Position = new Vector2(100, 200),
                Rotation = MathHelper.PiOver4,
                Scale = new Vector2(2, 3)
            };

            transform.RelativePosition.Should().Be(new Vector2(100, 200));
            transform.RelativeRotation.Should().BeApproximately(MathHelper.PiOver4);
            transform.RelativeScale.Should().BeApproximately(new Vector2(2, 3));
        }

        [TestMethod]
        public void SetRelativeValuesWithBase()
        {
            var baseTransform = new Transform
            {
                RelativePosition = new Vector2(50, 0),
                RelativeRotation = MathHelper.PiOver2,
                RelativeScale = new Vector2(2)
            };

            var transform = new Transform
            {
                BaseTransform = baseTransform,
                RelativePosition = new Vector2(50, 0),
                RelativeRotation = MathHelper.PiOver4,
                RelativeScale = new Vector2(0.5f)
            };

            transform.Position.Should().BeApproximately(new Vector2(50, 100));
            transform.Rotation.Should().BeApproximately(MathHelper.PiOver4 * 3);
            transform.Scale.Should().Be(Vector2.One);
        }

        [TestMethod]
        public void SetRealValuesWithBase()
        {
            var baseTransform = new Transform
            {
                Position = new Vector2(50, 0),
                Rotation = MathHelper.PiOver2,
                Scale = new Vector2(2)
            };

            var transform = new Transform
            {
                BaseTransform = baseTransform,
                Position = new Vector2(50, 100),
                Rotation = MathHelper.PiOver4 * 3,
                Scale = Vector2.One
            };

            transform.RelativePosition.Should().BeApproximately(new Vector2(50, 0));
            transform.RelativeRotation.Should().BeApproximately(MathHelper.PiOver4);
            transform.RelativeScale.Should().Be(new Vector2(0.5f));
        }

        [TestMethod]
        public void ChangeBaseValues()
        {
            var baseTransform = new Transform
            {
                RelativePosition = new Vector2(50, 0),
                RelativeRotation = MathHelper.PiOver2,
                RelativeScale = new Vector2(2)
            };

            var transform = new Transform
            {
                RelativePosition = new Vector2(50, 0),
                RelativeRotation = MathHelper.PiOver4,
                RelativeScale = new Vector2(0.5f),
                BaseTransform = baseTransform
            };

            transform.Position.Should().BeApproximately(new Vector2(50, 100));
            transform.Rotation.Should().BeApproximately(MathHelper.PiOver4 * 3);
            transform.Scale.Should().Be(Vector2.One);

            baseTransform.RelativePosition = new Vector2(0, 20);
            baseTransform.RelativeRotation = MathHelper.Pi;
            baseTransform.RelativeScale = new Vector2(0.5f);

            transform.Position.Should().BeApproximately(new Vector2(-25, 20));
            transform.Rotation.Should().BeApproximately(MathHelper.PiOver4 * 5);
            transform.Scale.Should().Be(new Vector2(0.25f));
        }

        [TestMethod]
        public void ChangeBaseTransform()
        {
            var baseTransform = new Transform
            {
                RelativePosition = new Vector2(0, 20),
                RelativeRotation = MathHelper.Pi,
                RelativeScale = new Vector2(0.5f)
            };

            var transform = new Transform
            {
                RelativePosition = new Vector2(50, 0),
                RelativeRotation = MathHelper.PiOver4,
                RelativeScale = new Vector2(0.5f)
            };

            baseTransform.ValidateTransformations();
            transform.ValidateTransformations();

            transform.BaseTransform = baseTransform;

            transform.Position.Should().BeApproximately(new Vector2(-25, 20));
            transform.Rotation.Should().BeApproximately(MathHelper.PiOver4 * 5);
            transform.Scale.Should().Be(new Vector2(0.25f));
        }

        [TestMethod]
        public void ChangeRelativeValuesValidation()
        {
            var transform = new Transform();

            transform.Position = Vector2.One;
            transform.RelativeRotation = 1;

            transform.RelativePosition.Should().Be(Vector2.One);
            transform.RelativeRotation.Should().Be(1);
        }

        [TestMethod]
        public void RevalidateBaseTransform()
        {
            var baseTransform = new Transform { RelativePosition = new Vector2(50, 0) };
            var transform = new Transform { RelativePosition = new Vector2(50, 0) };

            transform.BaseTransform = baseTransform;

            transform.Position.Should().Be(new Vector2(100, 0));

            baseTransform.RelativePosition = new Vector2(100, 0);
            baseTransform.ValidateTransformations();

            transform.Position.Should().Be(new Vector2(150, 0));
        }

        [TestMethod]
        public void ChangeBaseTransformAfterChangeCurrent()
        {
            var baseTransform = new Transform();
            var transform = new Transform { BaseTransform = baseTransform };

            baseTransform.Position = new Vector2(100, 0);
            
            baseTransform.Position.Should().Be(new Vector2(100, 0));
            transform.Position.Should().Be(new Vector2(100, 0));

            transform.Position = new Vector2(150, -50);

            baseTransform.Position.Should().Be(new Vector2(100, 0));
            transform.Position.Should().Be(new Vector2(150, -50));

            baseTransform.Position = new Vector2(200, 50);

            baseTransform.Position.Should().Be(new Vector2(200, 50));
            transform.Position.Should().Be(new Vector2(250, 0));

            baseTransform.Position = new Vector2(-50, -20);
            transform.RelativePosition = new Vector2(50, 20);

            baseTransform.Position.Should().Be(new Vector2(-50, -20));
            transform.Position.Should().Be(Vector2.Zero);
        }
    }
}