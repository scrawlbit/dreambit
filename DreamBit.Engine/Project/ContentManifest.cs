using System.Collections.Generic;
using System.Text;

namespace DreamBit.Engine.Project
{
    /// <summary>
    /// Gera um arquivo de conteúdo do MonoGame (.mgcb) listando os assets do projeto,
    /// para o Content Pipeline (`dotnet mgcb`) produzir os .xnb otimizados para distribuição.
    ///
    /// Observação: no editor as imagens são carregadas direto (TextureCache), sem build;
    /// o .mgcb existe para builds/exports de conteúdo compilado.
    /// </summary>
    public static class ContentManifest
    {
        public const string FileName = "Content.mgcb";

        /// <param name="relativeImagePaths">Caminhos das imagens relativos à pasta do projeto.</param>
        /// <param name="platform">Plataforma alvo do MGCB (DesktopGL, Windows, etc.).</param>
        public static string Generate(IEnumerable<string> relativeImagePaths, string platform = "DesktopGL")
        {
            var sb = new StringBuilder();

            sb.AppendLine("#----------------------------- Global Properties ----------------------------#");
            sb.AppendLine();
            sb.AppendLine("/outputDir:bin/$(Platform)");
            sb.AppendLine("/intermediateDir:obj/$(Platform)");
            sb.AppendLine($"/platform:{platform}");
            sb.AppendLine("/config:");
            sb.AppendLine("/profile:Reach");
            sb.AppendLine("/compress:False");
            sb.AppendLine();
            sb.AppendLine("#---------------------------------- Content ---------------------------------#");

            foreach (var path in relativeImagePaths)
            {
                var normalized = path.Replace('\\', '/');
                sb.AppendLine();
                sb.AppendLine("/importer:TextureImporter");
                sb.AppendLine("/processor:TextureProcessor");
                sb.AppendLine("/processorParam:ColorKeyColor=255,0,255,255");
                sb.AppendLine("/processorParam:GenerateMipmaps=False");
                sb.AppendLine("/processorParam:PremultiplyAlpha=True");
                sb.AppendLine("/processorParam:TextureFormat=Color");
                sb.AppendLine($"/build:{normalized}");
            }

            return sb.ToString();
        }
    }
}
