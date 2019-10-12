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
    public class TextRendererTest
    {
        [TestMethod]
        public void SetText()
        {
            var component = new TextRenderer(null);

            component.Text = null;
            component.Text.Should().BeNull();

            component.Text = "";
            component.Text.Should().BeNull();

            component.Text = " ";
            component.Text.Should().BeNull();

            component.Text = "test 1";
            component.Text.Should().Be("test 1");

            component.Text = " test 2 ";
            component.Text.Should().Be("test 2");
        }

        [TestMethod]
        public void DrawWithoutFont()
        {
            var drawBatch = new DrawBatchMock();
            var component = new TextRenderer(drawBatch) { Text = "test" };

            component.Draw();

            drawBatch.DrawStringCount.Should().Be(0);
        }

        [TestMethod]
        public void DrawWithoutText()
        {
            var drawBatch = new DrawBatchMock();
            var font = new FontMock();
            var component = new TextRenderer(drawBatch) { Font = font };

            component.Draw();

            drawBatch.DrawStringCount.Should().Be(0);
        }

        [TestMethod]
        public void DrawText()
        {
            var drawBatch = new DrawBatchMock();
            var font = new FontMock();
            var component = new TextRenderer(drawBatch) { Font = font, Text = "test" };

            component.Initialize(new GameObjectMock());
            component.Draw();

            drawBatch.DrawStringCount.Should().Be(1);
        }

        [TestMethod]
        public void DumpTest()
        {
            var drawBatch = new DrawBatchMock();
            var component = new TextRenderer(drawBatch)
            {
                Color = Color.Red,
                FlipHorizontally = false,
                FlipVertically = true,
                Origin = Vector2.One
            };
        }
    }
}