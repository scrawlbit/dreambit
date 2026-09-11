using System;

namespace DreamBit.Studio.Hosting
{
    // Portado de ScrawlBit.MonoGame.Interop/Util/Disposer.cs
    internal static class Disposer
    {
        public static void RemoveAndDispose<T>(ref T? resource) where T : class, IDisposable
        {
            resource?.Dispose();
            resource = null;
        }
    }
}
