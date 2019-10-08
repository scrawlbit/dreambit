using DreamBit.Project;

namespace DreamBit.Pipeline.Helpers
{
    internal static class ContentImportHelper
    {
        public static string GetContentPath(this IProjectFile file)
        {
            return file.Location.Replace('\\', '/');
        }
    }
}
