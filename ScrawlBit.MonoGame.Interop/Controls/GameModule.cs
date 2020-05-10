using Microsoft.Xna.Framework.Graphics;
using ScrawlBit.MonoGame.Interop.Services;
using System;
using System.ComponentModel.Design;
using System.Windows.Input;

namespace ScrawlBit.MonoGame.Interop.Controls
{
    public abstract class GameModule
    {
        private bool _prepared;

        protected GraphicsDevice GraphicsDevice { get; private set; }
        protected SpriteBatch SpriteBatch { get; private set; }
        protected IServiceProvider ServiceProvider { get; private set; }

        internal void Prepare(GraphicsDeviceService graphicsDeviceService)
        {
            if (_prepared)
                return;

            _prepared = true;

            var graphicsDevice = graphicsDeviceService.GraphicsDevice;
            var container = new ServiceContainer();

            container.AddService(typeof(IGraphicsDeviceService), graphicsDeviceService);
            container.AddService(typeof(GraphicsDevice), graphicsDevice);

            GraphicsDevice = graphicsDevice;
            SpriteBatch = new SpriteBatch(graphicsDevice);
            ServiceProvider = container;
        }

        protected internal virtual void Initialize() { }
        protected internal virtual void Draw() { }

        protected internal virtual void OnRenderSizeChanged(RenderSizeChangedEventArgs args) { }

        protected internal virtual void OnKeyDown(KeyEventArgs args) { }
        protected internal virtual void OnKeyUp(KeyEventArgs args) { }

        protected internal virtual void OnMouseEnter(MouseEventArgs args) { }
        protected internal virtual void OnMouseLeave(MouseEventArgs args) { }
        protected internal virtual void OnMouseMove(MouseEventArgs args) { }
        protected internal virtual void OnMouseDown(MouseButtonEventArgs args) { }
        protected internal virtual void OnMouseUp(MouseButtonEventArgs args) { }
        protected internal virtual void OnMouseWheel(MouseWheelEventArgs args) { }
    }
}