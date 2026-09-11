using System;
using System.Windows;
using System.Windows.Interop;
using SharpDX.Direct3D9;

namespace DreamBit.Studio.Hosting
{
    // Portado de ScrawlBit.MonoGame.Interop/Services/DeviceService.cs.
    // Cria o contexto Direct3D9Ex usado para compartilhar a textura do MonoGame
    // com o D3DImage do WPF. Funciona igual em net48 e em net8.0-windows.
    internal static class DeviceService
    {
        private static int _activeClients;
        private static Direct3DEx? _d3DContext;
        private static DeviceEx? _d3DDevice;

        public static DeviceEx D3DDevice => _d3DDevice!;

        public static void StartD3D(Window parentWindow)
        {
            _activeClients++;

            if (_activeClients > 1)
                return;

            _d3DContext = new Direct3DEx();

            var presentParameters = new PresentParameters
            {
                Windowed = true,
                SwapEffect = SwapEffect.Discard,
                DeviceWindowHandle = new WindowInteropHelper(parentWindow).Handle,
                PresentationInterval = PresentInterval.Default
            };

            _d3DDevice = new DeviceEx(_d3DContext, 0, DeviceType.Hardware, IntPtr.Zero,
                CreateFlags.HardwareVertexProcessing | CreateFlags.Multithreaded | CreateFlags.FpuPreserve,
                presentParameters);
        }

        public static void EndD3D()
        {
            _activeClients--;
            if (_activeClients < 0)
                throw new InvalidOperationException();

            if (_activeClients != 0)
                return;

            Disposer.RemoveAndDispose(ref _d3DDevice);
            Disposer.RemoveAndDispose(ref _d3DContext);
        }
    }
}
