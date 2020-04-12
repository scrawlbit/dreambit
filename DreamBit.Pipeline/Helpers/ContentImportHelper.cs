using DreamBit.Project;
using System.IO;

namespace DreamBit.Pipeline.Helpers
{
    public static class ContentImportHelper
    {
        public static string AsImportPath(this string path)
        {
            return path.Replace('\\', '/');
        }
        public static string GetImportPath(this IProjectFile file)
        {
            return file.Location.AsImportPath();
        }

        public static string AsContentPath(this string path)
        {
            string folder = Path.GetDirectoryName(path);
            string file = Path.GetFileNameWithoutExtension(path);

            return Path.Combine(folder, file).AsImportPath();
        }
        public static string GetContentPath(this IProjectFile file)
        {
            return file.Location.AsContentPath();
        }
    }
}
