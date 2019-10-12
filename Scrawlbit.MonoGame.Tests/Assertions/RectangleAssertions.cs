using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Xna.Framework;

namespace Scrawlbit.MonoGame.Tests.Assertions
{
    public class RectangleAssertions
    {
        public RectangleAssertions(Rectangle value)
        {
            Subject = value;
        }

        public Rectangle Subject { get; }

        public AndConstraint<RectangleAssertions> Be(Rectangle expectedValue, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .ForCondition(Subject == expectedValue)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:value} to be {0}{reason}, but found {1}.", expectedValue, Subject);

            return new AndConstraint<RectangleAssertions>(this);
        }
        public AndConstraint<RectangleAssertions> Be(int expectedX, int expectedY, int expectedWidth, int expectedHeight, string because = "", params object[] becauseArgs)
        {
            return Be(new Rectangle(expectedX, expectedY, expectedWidth, expectedHeight), because, becauseArgs);
        }
    }
}