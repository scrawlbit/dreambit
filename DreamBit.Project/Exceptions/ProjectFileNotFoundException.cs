using System;

namespace DreamBit.Project.Exceptions
{
    public class ProjectFileNotFoundException : Exception
    {
        public ProjectFileNotFoundException(string path) : base($"There is not a project file in the path {path}")
        {
        }
    }
}