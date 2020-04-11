using System;
using System.Collections.Generic;
using System.Linq;

namespace DreamBit.Project.Helpers
{
    public static class ProjectFileHelper
    {
        internal static bool IsPathIncluded(this IEnumerable<ProjectFile> files, string path)
        {
            return files.Any(f => f.Path == path);
        }

        public static ProjectFile GetByPath(this IEnumerable<ProjectFile> files, string path)
        {
            return files.SingleOrDefault(f => f.Path == path);
        }
        public static ProjectFile GetById(this IEnumerable<ProjectFile> files, Guid id)
        {
            return files.SingleOrDefault(f => f.Id == id);
        }
    }
}
