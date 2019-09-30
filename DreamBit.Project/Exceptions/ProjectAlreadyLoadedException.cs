using System;

namespace DreamBit.Project.Exceptions
{
    public class ProjectAlreadyLoadedException : Exception
    {
        public ProjectAlreadyLoadedException() : base("The project is already loaded")
        {
        }
    }
}