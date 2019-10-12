using DreamBit.Game.Drawing;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DreamBit.Game.Tests.Drawing
{
    [TestClass]
    public class DrawBatchDefinitionsTest
    {
        [TestMethod]
        public void DefaultValues()
        {
            var definition = new DrawBatchDefinition();

            definition.SortMode.Should().Be(SpriteSortMode.Deferred);
            definition.BlendState.Should().Be(BlendState.AlphaBlend);
            definition.SamplerState.Should().Be(SamplerState.LinearClamp);
            definition.DepthStencilState.Should().Be(DepthStencilState.None);
            definition.RasterizerState.Should().Be(RasterizerState.CullCounterClockwise);
            definition.Effect.Should().BeNull();
            definition.TransformMatrix.Should().BeNull();
        }

        [TestMethod]
        public void DefaultComparision()
        {
            var definitions1 = new DrawBatchDefinition();
            var definitions2 = new DrawBatchDefinition();

            definitions1.Should().Be(definitions2);
            definitions1.GetHashCode().Should().Be(definitions2.GetHashCode());
        }

        [TestMethod]
        public void ValuesComparision()
        {
            var definitions1 = new DrawBatchDefinition(blendState: BlendState.Additive, transformMatrix: Matrix.Identity);
            var definitions2 = new DrawBatchDefinition(blendState: BlendState.Additive, transformMatrix: Matrix.Identity);

            definitions1.Should().Be(definitions2);
            definitions1.GetHashCode().Should().Be(definitions2.GetHashCode());
        }

        [TestMethod]
        public void ChangeComparision()
        {
            var definitions1 = new DrawBatchDefinition(blendState: BlendState.Additive);
            var definitions2 = new DrawBatchDefinition(samplerState: SamplerState.LinearClamp);
            
            definitions1.Should().NotBe(definitions2);
            definitions1.GetHashCode().Should().NotBe(definitions2.GetHashCode());
        }

        [TestMethod]
        public void OtherObject()
        {
            ((object)0).Should().NotBe(new DrawBatchDefinition());
            ((object)0).GetHashCode().Should().NotBe(new DrawBatchDefinition().GetHashCode());
        }
    }
}