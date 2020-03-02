using DreamBit.Game.Elements;
using DreamBit.Game.Elements.Components;
using DreamBit.Game.Tests.Mocks.Content;
using DreamBit.Game.Tests.Mocks.Drawing;
using DreamBit.Game.Tests.Mocks.Elements;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Game.Tests.Elements.Components
{
    [TestClass]
    public class ImageRendererTest
    {
        [TestMethod]
        public void DrawWithoutImage()
        {
            var drawBatch = new DrawBatchMock();
            var component = new ImageRenderer(drawBatch);

            component.Draw();

            drawBatch.DrawCount.Should().Be(0);
        }

        [TestMethod]
        public void DrawWithImageNotLoaded()
        {
            var drawBatch = new DrawBatchMock();
            var component = new ImageRenderer(drawBatch);

            component.Draw();

            drawBatch.DrawCount.Should().Be(0);
        }

        [TestMethod]
        public void DrawWithImageLoaded()
        {
            var drawBatch = new DrawBatchMock();
            var image = new ImageMock();
            var component = new ImageRenderer(drawBatch) { Image = image };

            component.Initialize(new GameObjectMock());
            component.Draw();

            drawBatch.DrawCount.Should().Be(1);
        }
    }
}