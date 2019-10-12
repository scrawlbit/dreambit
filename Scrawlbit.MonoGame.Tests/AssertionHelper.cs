using Microsoft.Xna.Framework;
using Scrawlbit.MonoGame.Tests.Assertions;

namespace Scrawlbit.MonoGame.Tests
{
    public static class AssertionHelper
    {
        public static Vector2Assertions Should(this Vector2 actualValue)
        {
            return new Vector2Assertions(actualValue);
        }
        public static RectangleAssertions Should(this Rectangle actualValue)
        {
            return new RectangleAssertions(actualValue);
        }
    }
}