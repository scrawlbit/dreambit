using DreamBit.Game.Elements;
using DreamBit.Game.Tests.Mocks.Elements;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DreamBit.Game.Tests.Elements
{
    [TestClass]
    public class SceneTest
    {
        private Scene _scene;
        private GameObjectMock _gameObject;
        private GameObjectComponentMock _component;
        private GameObjectMock _child;
        private GameObjectComponentMock _childComponent;

        [TestInitialize]
        public void Initialize()
        {
            _scene = new Scene();
            _gameObject = new GameObjectMock();
            _component = new GameObjectComponentMock();
            _child = new GameObjectMock();
            _childComponent = new GameObjectComponentMock();

            _scene.GameObjects.Add(_gameObject);
            _gameObject.Components.Add(_component);
            _gameObject.Children.Add(_child);
            _child.Components.Add(_childComponent);
        }

        [TestMethod]
        public void InitializeBeforeLoop()
        {
            _gameObject.IsVisible = false;
            _scene.Initialize();
            
            _component.StartCalled.Should().BeTrue();
            _childComponent.StartCalled.Should().BeTrue();
        }

        [TestMethod]
        public void Loop()
        {
            _scene.Update();
            _scene.PostUpdate();
            _scene.Draw();

            _component.StartCalled.Should().BeTrue();
            _component.UpdateCalled.Should().BeTrue();
            _component.PostUpdateCalled.Should().BeTrue();
            _component.DrawCalled.Should().BeTrue();

            _childComponent.StartCalled.Should().BeTrue();
            _childComponent.UpdateCalled.Should().BeTrue();
            _childComponent.PostUpdateCalled.Should().BeTrue();
            _childComponent.DrawCalled.Should().BeTrue();
        }

        [TestMethod]
        public void UpdateAndDrawVisibleObjects()
        {
            var parentComponent = new GameObjectComponentMock();
            var child1Component = new GameObjectComponentMock();
            var child2Component = new GameObjectComponentMock();
            var child3Component = new GameObjectComponentMock();

            var parent = new GameObjectMock
            {
                Components = { parentComponent },
                Children =
                {
                    new GameObjectMock
                    {
                        Components = { child1Component },
                    },
                    new GameObjectMock
                    {
                        IsVisible = false,
                        Components = { child2Component },
                        Children =
                        {
                            new GameObjectMock
                            {
                                Components = { child3Component }
                            }
                        }
                    }
                }
            };

            _scene = new Scene();
            _scene.GameObjects.Add(parent);

            _scene.Update();

            parentComponent.UpdateCalled.Should().BeTrue();
            child1Component.UpdateCalled.Should().BeTrue();
            child2Component.UpdateCalled.Should().BeFalse();
            child2Component.UpdateCalled.Should().BeFalse();

            _scene.PostUpdate();

            parentComponent.PostUpdateCalled.Should().BeTrue();
            child1Component.PostUpdateCalled.Should().BeTrue();
            child2Component.PostUpdateCalled.Should().BeFalse();
            child2Component.PostUpdateCalled.Should().BeFalse();

            _scene.Draw();

            parentComponent.DrawCalled.Should().BeTrue();
            child1Component.DrawCalled.Should().BeTrue();
            child2Component.DrawCalled.Should().BeFalse();
            child2Component.DrawCalled.Should().BeFalse();
        }
    }
}