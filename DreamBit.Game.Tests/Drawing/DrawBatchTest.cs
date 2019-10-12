using DreamBit.Game.Drawing;
using DreamBit.Game.Tests.Mocks.Drawing;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework.Graphics;

namespace DreamBit.Game.Tests.Drawing
{
    [TestClass]
    public class DrawBatchTest
    {
        [TestMethod]
        public void SingleBatchCall()
        {
            var drawBatchService = new DrawBatchServiceMock();
            var drawBatch = (IDrawBatch)new DrawBatch(drawBatchService);

            drawBatch.Begin();
            
            drawBatchService.BeginCount.Should().Be(1);
            drawBatchService.EndCount.Should().Be(0);

            drawBatch.End();
            
            drawBatchService.BeginCount.Should().Be(1);
            drawBatchService.EndCount.Should().Be(1);
        }

        [TestMethod]
        public void MultipleBatchCallWithSameDefinition()
        {
            var drawBatchService = new DrawBatchServiceMock();
            var drawBatch = (IDrawBatch)new DrawBatch(drawBatchService);

            drawBatch.Begin();
            drawBatch.Begin();

            drawBatchService.LastBeginDefinition.Should().Be(new DrawBatchDefinition());
            drawBatchService.BeginCount.Should().Be(1);
            drawBatchService.EndCount.Should().Be(0);

            drawBatch.End();
            drawBatch.End();

            drawBatchService.BeginCount.Should().Be(1);
            drawBatchService.EndCount.Should().Be(1);
        }

        [TestMethod]
        public void MultipleBatchCallWithDifferentDefinition()
        {
            var drawBatchService = new DrawBatchServiceMock();
            var drawBatch = (IDrawBatch)new DrawBatch(drawBatchService);
            
            drawBatch.Begin(SpriteSortMode.BackToFront);

            drawBatchService.LastBeginDefinition.Should().Be(new DrawBatchDefinition(SpriteSortMode.BackToFront));
            drawBatchService.BeginCount.Should().Be(1);
            drawBatchService.EndCount.Should().Be(0);

            drawBatch.Begin();

            drawBatchService.LastBeginDefinition.Should().Be(new DrawBatchDefinition());
            drawBatchService.BeginCount.Should().Be(2);
            drawBatchService.EndCount.Should().Be(1);

            drawBatch.Begin();

            drawBatchService.LastBeginDefinition.Should().Be(new DrawBatchDefinition());
            drawBatchService.BeginCount.Should().Be(2);
            drawBatchService.EndCount.Should().Be(1);

            drawBatch.Begin(depthStencilState: DepthStencilState.DepthRead);

            drawBatchService.LastBeginDefinition.Should().Be(new DrawBatchDefinition(depthStencilState: DepthStencilState.DepthRead));
            drawBatchService.BeginCount.Should().Be(3);
            drawBatchService.EndCount.Should().Be(2);

            drawBatch.End();

            drawBatchService.LastBeginDefinition.Should().Be(new DrawBatchDefinition());
            drawBatchService.BeginCount.Should().Be(4);
            drawBatchService.EndCount.Should().Be(3);

            drawBatch.End();

            drawBatchService.LastBeginDefinition.Should().Be(new DrawBatchDefinition());
            drawBatchService.BeginCount.Should().Be(4);
            drawBatchService.EndCount.Should().Be(3);

            drawBatch.End();

            drawBatchService.LastBeginDefinition.Should().Be(new DrawBatchDefinition(SpriteSortMode.BackToFront));
            drawBatchService.BeginCount.Should().Be(5);
            drawBatchService.EndCount.Should().Be(4);

            drawBatch.End();

            drawBatchService.BeginCount.Should().Be(5);
            drawBatchService.EndCount.Should().Be(5);
        }
    }
}