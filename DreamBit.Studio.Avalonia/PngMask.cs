using System;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace DreamBit.Studio.Avalonia
{
    /// <summary>
    /// Decodifica um PNG (via Avalonia) numa máscara de opacidade — usada pela autodetecção de
    /// frames (<see cref="DreamBit.Engine.Rendering.FrameDetector"/>), que só precisa saber
    /// quais pixels são visíveis, não a cor.
    /// </summary>
    public static class PngMask
    {
        /// <summary>Carrega as cores (RGBA) do PNG. Retorna false se falhar.</summary>
        public static bool TryLoadColors(string path, out XnaColor[] colors, out int width, out int height)
        {
            colors = Array.Empty<XnaColor>();
            width = height = 0;
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return false;
            try
            {
                using var bmp = new Bitmap(path);
                width = bmp.PixelSize.Width;
                height = bmp.PixelSize.Height;
                if (width <= 0 || height <= 0)
                    return false;

                int stride = width * 4, size = stride * height;
                IntPtr buffer = Marshal.AllocHGlobal(size);
                try
                {
                    bmp.CopyPixels(new PixelRect(0, 0, width, height), buffer, size, stride);
                    var bytes = new byte[size];
                    Marshal.Copy(buffer, bytes, 0, size);
                    colors = new XnaColor[width * height];
                    for (int i = 0; i < colors.Length; i++) // BGRA -> RGBA
                        colors[i] = new XnaColor(bytes[i * 4 + 2], bytes[i * 4 + 1], bytes[i * 4], bytes[i * 4 + 3]);
                }
                finally { Marshal.FreeHGlobal(buffer); }
                return true;
            }
            catch { return false; }
        }

        /// <summary>Carrega a máscara (true = pixel com alfa acima do limiar). Retorna false se falhar.</summary>
        public static bool TryLoad(string path, out bool[] opaque, out int width, out int height, byte alphaThreshold = 16)
        {
            opaque = Array.Empty<bool>();
            width = height = 0;
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return false;

            try
            {
                using var bmp = new Bitmap(path);
                width = bmp.PixelSize.Width;
                height = bmp.PixelSize.Height;
                if (width <= 0 || height <= 0)
                    return false;

                int stride = width * 4;
                int size = stride * height;
                IntPtr buffer = Marshal.AllocHGlobal(size);
                try
                {
                    bmp.CopyPixels(new PixelRect(0, 0, width, height), buffer, size, stride);
                    var bytes = new byte[size];
                    Marshal.Copy(buffer, bytes, 0, size);

                    opaque = new bool[width * height];
                    for (int i = 0; i < opaque.Length; i++)
                        opaque[i] = bytes[i * 4 + 3] > alphaThreshold; // canal A (BGRA)
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
