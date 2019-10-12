using DreamBit.Game.Content.Loaders;
using DreamBit.Game.Tests.Mocks.Reading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DreamBit.Game.Tests.Content
{
    [TestClass]
    public class SceneLoaderTest
    {
        [TestMethod]
        public void LoadScene()
        {
            var dataReader = new DataReaderMock();
            var loader = new SceneLoader();
            
            var content = loader.Load(null, "test", null, dataReader);

            content.Should().Be(dataReader.Content);
            dataReader.AssetRequested.Should().Be("test.scene");
        }
    }
}