using DreamBit.Game.Tests.Implementations.Elements;
using DreamBit.Game.Tests.Mocks.Elements;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DreamBit.Game.Tests.Elements.Components
{
    [TestClass]
    public class GameObjectComponentTest
    {
        [TestMethod]
        public void StartOnce()
        {
            var gameObject = new GameObjectMock();
            var component = new GameObjectComponentImplementation();

            component.Started.Should().BeFalse();

            component.Initialize(gameObject);
            component.StartCount.Should().Be(1);
            component.Started.Should().BeTrue();

            component.Initialize(gameObject);
            component.StartCount.Should().Be(1);
            component.Started.Should().BeTrue();
        }
    }
}