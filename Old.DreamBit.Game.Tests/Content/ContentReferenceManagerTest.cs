//using System;
//using DreamBit.Game.Content;
//using DreamBit.Game.Data;
//using DreamBit.Game.Tests.Mocks.Content.References;
//using DreamBit.Game.Tests.Mocks.Data;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace DreamBit.Game.Tests.Content
//{
//    [TestClass]
//    public class ContentReferenceManagerTest
//    {
//        [TestMethod]
//        public void LoadImage()
//        {
//            var fileId = Guid.NewGuid();
//            var contentReference = new ImageReferenceMock { FileId = fileId, AssetName = "image" };
//            var gameContent = new GameContent { FileId = fileId, ContentPath = "image" };
//            var gameData = new GameDataMock { ContentPaths = { gameContent } };
//            var factory = new ContentReferenceFactoryMock { ContentReference = contentReference };
//            var contentManager = new ContentReferenceManager(factory, gameData);

//            var image1 = contentManager.LoadImage("image");
//            var image2 = contentManager.LoadImage(fileId);

//            AssertHelper.AreEqual(factory.ContentCreatedCount, 1);
//            AssertHelper.AreEqual(image1, contentReference);
//            AssertHelper.AreEqual(image1, image2);
//        }
//    }
//}