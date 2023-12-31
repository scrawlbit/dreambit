﻿using Microsoft.Xna.Framework.Graphics;

namespace ScrawlBit.MonoGame.Interop.Controls
{
    /// <summary>
    /// Provides data for the Draw event.
    /// </summary>
    public sealed class DrawEventArgs : GraphicsDeviceEventArgs
    {
        private readonly DrawingSurface _drawingSurface;

        public DrawEventArgs(DrawingSurface drawingSurface, GraphicsDevice graphicsDevice)
            : base(graphicsDevice)
        {
            _drawingSurface = drawingSurface;
        }

        public void InvalidateSurface()
        {
            _drawingSurface.Invalidate();
        }
    }
}