using System;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Xna.Framework;

namespace Scrawlbit.MonoGame.Tests.Assertions
{
    public class Vector2Assertions
    {
        public Vector2Assertions(Vector2 value)
        {
            Subject = value;
        }

        public Vector2 Subject { get; }

        public AndConstraint<Vector2Assertions> Be(Vector2 expectedValue, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .ForCondition(Subject == expectedValue)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:value} to be {0}{reason}, but found {1}.", expectedValue, Subject);

            return new AndConstraint<Vector2Assertions>(this);
        }
        public AndConstraint<Vector2Assertions> Be(float expectedX, float expectedY, string because = "", params object[] becauseArgs)
        {
            return Be(new Vector2(expectedX, expectedY), because, becauseArgs);
        }
        public AndConstraint<Vector2Assertions> Be(float expectedValues, string because = "", params object[] becauseArgs)
        {
            return Be(expectedValues, expectedValues, because, becauseArgs);
        }

        public AndConstraint<Vector2Assertions> BeApproximately(Vector2 expectedValue, string because = "", params object[] becauseArgs)
        {
            var actualDifference = expectedValue - Subject;
            var xValid = Math.Abs(actualDifference.X) <= Constants.CommonPrecision;
            var yValid = Math.Abs(actualDifference.Y) <= Constants.CommonPrecision;

            Execute.Assertion
                .ForCondition(xValid && yValid)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:value} {0} to approximate {1} +/- {2}{reason}, but it differed by {3}",
                    Subject, expectedValue, Constants.CommonPrecision, actualDifference);

            return new AndConstraint<Vector2Assertions>(this);
        }
        public AndConstraint<Vector2Assertions> BeApproximately(float expectedX, float expectedY, string because = "", params object[] becauseArgs)
        {
            return BeApproximately(new Vector2(expectedX, expectedY), because, becauseArgs);
        }
        public AndConstraint<Vector2Assertions> BeApproximately(float expectedValues, string because = "", params object[] becauseArgs)
        {
            return BeApproximately(expectedValues, expectedValues, because, becauseArgs);
        }
    }
}