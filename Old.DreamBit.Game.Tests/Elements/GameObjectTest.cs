using DreamBit.Game.Elements;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Game.Tests.Elements
{
    [TestClass]
    public class GameObjectTest
    {
        [TestMethod]
        public void InitialValues()
        {
            var gameObject = new GameObject();

            gameObject.Name.Should().BeNull();
            gameObject.IsVisible.Should().BeTrue();
            gameObject.Parent.Should().Be(null);
            gameObject.Transform.Position.Should().Be(Vector2.Zero);
            gameObject.Transform.Rotation.Should().Be(0);
            gameObject.Transform.Scale.Should().Be(Vector2.One);
            gameObject.Children.Should().BeEmpty();
            gameObject.Components.Should().BeEmpty();
        }

        [TestMethod]
        public void ChangeChildren()
        {
            var parent1 = new GameObject();
            var parent2 = new GameObject();
            var gameObject = new GameObject();

            parent1.AddChild(gameObject);
            parent1.Children.Should().Contain(gameObject);
            gameObject.Parent.Should().Be(parent1);

            parent2.RemoveChild(gameObject);
            parent2.Children.Should().NotContain(gameObject);
            parent1.Children.Should().Contain(gameObject);
            gameObject.Parent.Should().Be(parent1);

            parent2.AddChild(gameObject);
            parent1.Children.Should().NotContain(gameObject);
            parent2.Children.Should().Contain(gameObject);
            gameObject.Parent.Should().Be(parent2);

            parent2.RemoveChild(gameObject);
            parent2.Children.Should().NotContain(gameObject);
            gameObject.Parent.Should().Be(null);
        }

        [TestMethod]
        public void ChangeParent()
        {
            var parent1 = new GameObject();
            var parent2 = new GameObject();
            var gameObject = new GameObject();

            gameObject.Parent = parent1;
            parent1.Children.Should().Contain(gameObject);
            gameObject.Parent.Should().Be(parent1);

            gameObject.Parent = parent2;
            parent1.Children.Should().NotContain(gameObject);
            parent2.Children.Should().Contain(gameObject);
            gameObject.Parent.Should().Be(parent2);

            gameObject.Parent = null;
            parent2.Children.Should().NotContain(gameObject);
            gameObject.Parent.Should().BeNull();
        }

        [TestMethod]
        public void BaseTransformMatchesParentTransform()
        {
            var parent1 = new GameObject();
            var parent2 = new GameObject();
            var gameObject = new GameObject();

            parent1.AddChild(gameObject);
            gameObject.Transform.BaseTransform.Should().Be(parent1.Transform);

            gameObject.Parent = parent2;
            gameObject.Transform.BaseTransform.Should().Be(parent2.Transform);

            gameObject.Parent = null;
            gameObject.Transform.BaseTransform.Should().BeNull();
        }
    }
}