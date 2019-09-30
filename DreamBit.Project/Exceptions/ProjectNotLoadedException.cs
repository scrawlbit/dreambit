using System;

namespace DreamBit.Project.Exceptions
{
    public class ProjectNotLoadedException : Exception
    {
        public ProjectNotLoadedException() : base("The project is not loaded")
        {
        }
    }
}