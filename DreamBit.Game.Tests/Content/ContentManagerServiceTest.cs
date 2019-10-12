using System;
using DreamBit.Game.Content;
using DreamBit.Game.Data;
using DreamBit.Game.Exceptions;
using DreamBit.Game.Tests.Mocks.Content;
using DreamBit.Game.Tests.Mocks.Data;
using DreamBit.Game.Tests.Mocks.Reading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DreamBit.Game.Tests.Content
{
    [TestClass]
    public class ContentManagerServiceTest
    {
        private ContentManagerService _contentManager;
        private GameDataMock _gameData;
        private ContentLoaderMock _contentLoader;

        [TestInitialize]
        public void Initialize()
        {
            _gameData = new GameDataMock();
            _contentLoader = new ContentLoaderMock();
            _contentManager = new ContentManagerService(new DataReaderMock(), _gameData, _contentLoader);
        }

        [TestMethod]
        public void LoadContentByName()
        {
            _contentLoader.ContentType = typeof(IFont);
            _contentLoader.ContentToLoad = new FontMock();

            var content = _contentManager.Load<IFont>("Fonts/Segoe");

            content.Should().Be(_contentLoader.ContentToLoad);
            _contentLoader.Request.fileId.Should().BeNull();
            _contentLoader.Request.AssetName.Should().Be("Fonts/Segoe");
        }

        [TestMethod]
        [ExpectedException(typeof (ContentNotRegisteredException))]
        public void LoadContentNotRegistered()
        {
            _contentManager.Load<IFont>(Guid.NewGuid());
        }

        [TestMethod]
        public void LoadContentById()
        {
            var id = Guid.NewGuid();
            const string path = "Fonts/Segoe";
            
            _gameData.ContentPaths.Add(new GameContent { FileId = id, ContentPath = path });
            _contentLoader.ContentType = typeof(IFont);
            _contentLoader.ContentToLoad = new FontMock();

            var content = _contentManager.Load<IFont>(id);

            content.Should().Be(_contentLoader.ContentToLoad);
            _contentLoader.Request.fileId.Should().Be(id);
            _contentLoader.Request.AssetName.Should().Be(path);
        }

        [TestMethod]
        public void GetContentAlreadyLoaded()
        {
            _contentLoader.ContentType = typeof(IFont);
            _contentLoader.ContentToLoad = new FontMock();

            var content1 = _contentManager.Load<IFont>("Fonts/Segoe");

            _contentLoader.ContentToLoad = null;
            var content2 = _contentManager.Load<IFont>("Fonts/Segoe");

            content1.Should().Be(content2);
        }
    }
}