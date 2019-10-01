using System;
using System.ComponentModel.Design;
using DreamBit.Extension.Interop.Services;
using Microsoft.Xna.Framework.Graphics;

namespace DreamBit.Extension.Interop.Controls
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

        public virtual void Initialize()
        {
            LoadContent();
        }
        public virtual void LoadContent() { }
        public virtual void Draw() { }
    }
}