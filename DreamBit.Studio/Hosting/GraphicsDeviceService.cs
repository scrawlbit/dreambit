using System;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Xna.Framework.Graphics;

namespace DreamBit.Studio.Hosting
{
    // Portado de ScrawlBit.MonoGame.Interop/Services/GraphicsDeviceService.cs.
    // Singleton que cria e compartilha um único GraphicsDevice entre todas as
    // superfícies de desenho. Removidos os atributos [Export] do MEF (não usados aqui).
    internal sealed class GraphicsDeviceService : IGraphicsDeviceService
    {
        private static GraphicsDeviceService? _instance;
        private static int _referenceCount;

        private GraphicsDevice? _graphicsDevice;
        private PresentationParameters? _parameters;

        private static GraphicsDeviceService Instance => _instance ??= new GraphicsDeviceService();

        public GraphicsDevice GraphicsDevice
        {
            get
            {
                EnsureGraphicsDevice();
                return _graphicsDevice!;
            }
        }

        public event EventHandler<EventArgs>? DeviceCreated;
        public event EventHandler<EventArgs>? DeviceDisposing;
        public event EventHandler<EventArgs>? DeviceReset;
        public event EventHandler<EventArgs>? DeviceResetting;

        private GraphicsDeviceService() { }

        private void EnsureGraphicsDevice()
        {
            if (_graphicsDevice != null)
                return;

            var handle = new WindowInteropHelper(Application.Current.MainWindow!).Handle;
            CreateDevice(handle, 1, 1);
        }

        private void CreateDevice(IntPtr windowHandle, int width, int height)
        {
            _parameters = new PresentationParameters
            {
                BackBufferWidth = Math.Max(width, 1),
                BackBufferHeight = Math.Max(height, 1),
                BackBufferFormat = SurfaceFormat.Color,
                DepthStencilFormat = DepthFormat.Depth24,
                DeviceWindowHandle = windowHandle,
                PresentationInterval = PresentInterval.Immediate,
                IsFullScreen = false
            };

            _graphicsDevice = new GraphicsDevice(
                GraphicsAdapter.DefaultAdapter,
                GraphicsProfile.HiDef,
                _parameters);

            DeviceCreated?.Invoke(this, EventArgs.Empty);
        }

        public static GraphicsDeviceService AddRef(int width, int height)
        {
            var singleton = Instance;

            if (Interlocked.Increment(ref _referenceCount) == 1)
                singleton.EnsureGraphicsDevice();

            return singleton;
        }

        public void Release(bool disposing)
        {
            if (Interlocked.Decrement(ref _referenceCount) == 0)
            {
                if (disposing)
                {
                    DeviceDisposing?.Invoke(this, EventArgs.Empty);
                    _graphicsDevice?.Dispose();
                }

                _graphicsDevice = null;
            }
        }

        public void ResetDevice(int width, int height)
        {
            if (_parameters == null)
                return;

            int newWidth = Math.Max(_parameters.BackBufferWidth, width);
            int newHeight = Math.Max(_parameters.BackBufferHeight, height);

            if (newWidth != _parameters.BackBufferWidth || newHeight != _parameters.BackBufferHeight)
            {
                DeviceResetting?.Invoke(this, EventArgs.Empty);

                _parameters.BackBufferWidth = newWidth;
                _parameters.BackBufferHeight = newHeight;

                DeviceReset?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
