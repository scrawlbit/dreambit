using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Xna.Framework.Graphics;

namespace DreamBit.Studio.Hosting
{
    /// <summary>
    /// Superfície WPF que hospeda um render MonoGame ao vivo via D3DImage.
    /// Portado de ScrawlBit.MonoGame.Interop/Controls/DrawingSurface.cs, adaptado
    /// para net8.0-windows e com um evento Draw que já entrega o delta de tempo,
    /// para o editor rodar um game loop contínuo sem depender do Visual Studio.
    /// </summary>
    public sealed class MonoGameSurface : ContentControl, IDisposable
    {
        public event EventHandler<GraphicsDevice>? LoadContent;
        public event EventHandler<SurfaceDrawEventArgs>? Draw;

        private GraphicsDeviceService? _graphicsDeviceService;
        private readonly D3DImage _d3dImage;
        private readonly Image _image;
        private RenderTarget2D? _renderTarget;
        private SharpDX.Direct3D9.Texture? _renderTargetD3D9;

        private bool _contentNeedsRefresh;
        private readonly Stopwatch _clock = new();
        private TimeSpan _lastTick;

        /// <summary>Redesenha a cada frame do WPF (loop contínuo). Ligado por padrão no editor.</summary>
        public bool AlwaysRefresh { get; set; } = true;

        public GraphicsDevice? GraphicsDevice => _graphicsDeviceService?.GraphicsDevice;

        public MonoGameSurface()
        {
            _d3dImage = new D3DImage();
            _image = new Image { Source = _d3dImage, Stretch = Stretch.None };
            AddChild(_image);

            _d3dImage.IsFrontBufferAvailableChanged += OnFrontBufferAvailableChanged;

            Focusable = true;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            RemoveBackBufferReference();
            _contentNeedsRefresh = true;
            base.OnRenderSizeChanged(sizeInfo);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_graphicsDeviceService != null)
                return;

            DeviceService.StartD3D(Window.GetWindow(this)!);

            _graphicsDeviceService = GraphicsDeviceService.AddRef(1, 1);
            _graphicsDeviceService.DeviceResetting += OnDeviceResetting;

            LoadContent?.Invoke(this, GraphicsDevice!);
            EnsureRenderTarget();

            _clock.Start();
            _lastTick = _clock.Elapsed;
            CompositionTarget.Rendering += OnRendering;
            _contentNeedsRefresh = true;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_graphicsDeviceService == null)
                return;

            RemoveBackBufferReference();
            CompositionTarget.Rendering -= OnRendering;

            _graphicsDeviceService.DeviceResetting -= OnDeviceResetting;
            _graphicsDeviceService.Release(true);
            _graphicsDeviceService = null;

            DeviceService.EndD3D();
        }

        private void OnDeviceResetting(object? sender, EventArgs e)
        {
            RemoveBackBufferReference();
            _contentNeedsRefresh = true;
        }

        private void RemoveBackBufferReference()
        {
            _renderTarget?.Dispose();
            _renderTarget = null;
            _renderTargetD3D9?.Dispose();
            _renderTargetD3D9 = null;

            _d3dImage.Lock();
            _d3dImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, IntPtr.Zero);
            _d3dImage.Unlock();
        }

        private void EnsureRenderTarget()
        {
            if (_renderTarget != null)
                return;

            int width = Math.Max(1, (int)ActualWidth);
            int height = Math.Max(1, (int)ActualHeight);

            _renderTarget = new RenderTarget2D(GraphicsDevice, width, height,
                false, SurfaceFormat.Bgra32, DepthFormat.Depth24Stencil8, 1,
                RenderTargetUsage.PlatformContents, true);

            var handle = _renderTarget.GetSharedHandle();
            if (handle == IntPtr.Zero)
                throw new InvalidOperationException("Não foi possível obter o handle compartilhado do render target.");

            _renderTargetD3D9 = new SharpDX.Direct3D9.Texture(DeviceService.D3DDevice,
                _renderTarget.Width, _renderTarget.Height, 1,
                SharpDX.Direct3D9.Usage.RenderTarget, SharpDX.Direct3D9.Format.A8R8G8B8,
                SharpDX.Direct3D9.Pool.Default, ref handle);

            using var surface = _renderTargetD3D9.GetSurfaceLevel(0);
            _d3dImage.Lock();
            _d3dImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, surface.NativePointer);
            _d3dImage.Unlock();
        }

        private void OnFrontBufferAvailableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_d3dImage.IsFrontBufferAvailable)
                _contentNeedsRefresh = true;
        }

        private void OnRendering(object? sender, EventArgs e)
        {
            if (!(_contentNeedsRefresh || AlwaysRefresh) || !BeginDraw())
                return;

            _contentNeedsRefresh = false;

            var now = _clock.Elapsed;
            double delta = (now - _lastTick).TotalSeconds;
            _lastTick = now;

            _d3dImage.Lock();
            EnsureRenderTarget();

            var device = GraphicsDevice!;
            device.SetRenderTarget(_renderTarget);
            device.Viewport = new Viewport(0, 0, Math.Max(1, (int)ActualWidth), Math.Max(1, (int)ActualHeight));

            Draw?.Invoke(this, new SurfaceDrawEventArgs(device, delta, (int)ActualWidth, (int)ActualHeight));

            device.Flush();
            _d3dImage.AddDirtyRect(new Int32Rect(0, 0, Math.Max(1, (int)ActualWidth), Math.Max(1, (int)ActualHeight)));
            _d3dImage.Unlock();

            device.SetRenderTarget(null);
        }

        private bool BeginDraw()
        {
            if (_graphicsDeviceService == null)
                return false;
            if (!_d3dImage.IsFrontBufferAvailable)
                return false;
            return HandleDeviceReset();
        }

        private bool HandleDeviceReset()
        {
            switch (GraphicsDevice!.GraphicsDeviceStatus)
            {
                case GraphicsDeviceStatus.Lost:
                    return false;
                case GraphicsDeviceStatus.NotReset:
                    _graphicsDeviceService!.ResetDevice((int)ActualWidth, (int)ActualHeight);
                    return false;
                default:
                    return true;
            }
        }

        public void Invalidate() => _contentNeedsRefresh = true;

        public void Dispose()
        {
            _renderTarget?.Dispose();
            _renderTargetD3D9?.Dispose();
            _graphicsDeviceService?.Release(true);
        }
    }

    public sealed class SurfaceDrawEventArgs : EventArgs
    {
        public SurfaceDrawEventArgs(GraphicsDevice device, double deltaSeconds, int width, int height)
        {
            GraphicsDevice = device;
            DeltaSeconds = deltaSeconds;
            Width = width;
            Height = height;
        }

        public GraphicsDevice GraphicsDevice { get; }
        public double DeltaSeconds { get; }
        public int Width { get; }
        public int Height { get; }
    }
}
