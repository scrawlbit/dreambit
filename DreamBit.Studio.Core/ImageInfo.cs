using System.IO;

namespace DreamBit.Studio
{
    /// <summary>Lê dimensões de imagens sem carregar em GPU (só o cabeçalho PNG).</summary>
    public static class ImageInfo
    {
        /// <summary>Largura/altura de um PNG lendo o chunk IHDR, ou (0,0) se falhar.</summary>
        public static (int Width, int Height) GetPngSize(string path)
        {
            try
            {
                using var stream = File.OpenRead(path);
                var header = new byte[24];
                if (stream.Read(header, 0, 24) < 24)
                    return (0, 0);

                // Assinatura PNG (8 bytes) + IHDR: length(4) + "IHDR"(4) + width(4) + height(4)
                int width = header[16] << 24 | header[17] << 16 | header[18] << 8 | header[19];
                int height = header[20] << 24 | header[21] << 16 | header[22] << 8 | header[23];
                return (width, height);
            }
            catch
            {
                return (0, 0);
            }
        }
    }
}
