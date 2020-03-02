using System;
using DreamBit.Game.Components;
using DreamBit.Game.Elements;
using DreamBit.Game.Tests.Mocks.Content;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DreamBit.Game.Tests.Components
{
    [TestClass]
    public class SceneManagerTest
    {
        private Scene _scene;
        private ContentReferenceManagerMock _contentReferenceManager;
        private ContentManagerMock _contentManager;
        private SceneManager _sceneManager;

        [TestInitialize]
        public void Initialize()
        {
            _scene = new Scene();

            _contentReferenceManager = new ContentReferenceManagerMock();
            _contentManager = new ContentManagerMock { ContentToLoad = _scene };
            _sceneManager = new SceneManager(_contentManager, _contentReferenceManager);
        }

        [TestMethod]
        public void InitialValues()
        {
            _sceneManager.OpenedScene.Should().BeNull();
        }

        [TestMethod]
        public void LoadWithFileId()
        {
            var fileId = Guid.NewGuid();

            _sceneManager.Load(fileId);

            _sceneManager.OpenedScene.Should().Be(_scene);
            _contentManager.Request.FileId.Should().Be(fileId);
        }

        [TestMethod]
        public void LoadWithAssetName()
        {
            _sceneManager.Load("scene");

            _sceneManager.OpenedScene.Should().Be(_scene);
            _contentManager.Request.AssetName.Should().Be("scene");
        }
    }
}