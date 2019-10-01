using Microsoft.VisualStudio;

namespace DreamBit.Extension.Helpers
{
    public static class ErrorHelper
    {
        public static void ThrowOnFailure(this int result)
        {
            ErrorHandler.ThrowOnFailure(result);
        }
    }
}
