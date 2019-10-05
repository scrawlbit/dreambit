using System.Collections.Generic;
using System.Linq;

namespace DreamBit.Project.Helpers
{
    internal static class ProjectFileHelper
    {
        public static bool ContainsPath(this IEnumerable<ProjectFile> files, string path)
        {
            return files.Any(f => f.Path == path);
        }

        public static ProjectFile GetByPath(this IEnumerable<ProjectFile> files, string path)
        {
            return files.SingleOrDefault(f => f.Path == path);
        }
    }
}
