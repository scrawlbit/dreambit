using FluentAssertions;
using FluentAssertions.Numeric;

namespace Scrawlbit.MonoGame.Tests
{
    public static class FloatAssertionsHelper
    {
        public static AndConstraint<NumericAssertions<float>> BeApproximately(this NumericAssertions<float> assertions, float expectedValue, string because = "", params object[] becauseArgs)
        {
            return assertions.BeApproximately(expectedValue, Constants.CommonPrecision, because, becauseArgs);
        }
    }
}