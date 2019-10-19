using DreamBit.Project;

namespace DreamBit.Pipeline.Helpers
{
    internal static class ContentImportHelper
    {
        public static string AsContentPath(this string path)
        {
            return path.Replace('\\', '/');
        }
        public static string GetContentPath(this IProjectFile file)
        {
            return file.Location.AsContentPath();
        }
    }
}
