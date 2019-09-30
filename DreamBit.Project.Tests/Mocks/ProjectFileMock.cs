using System;

namespace DreamBit.Project.Mocks
{
    public class ProjectFileMock : ProjectFile
    {
        public ProjectFileMock(IProject project, Guid id, string type, string extension, string location)
        {
            Project = project;
            Id = id;
            Type = type;
            Extension = extension;
            Location = location;
        }
        public ProjectFileMock(IProject project, string type, string extension, string location)
        {
            Project = project;
            Type = type;
            Extension = extension;
            Location = location;
        }

        public override string Type { get; }
        public override string Extension { get; }

        public static ProjectFileMock Script(IProject project, Guid id, string location)
        {
            return new ProjectFileMock(project, id, "Script", "cs", location);
        }
        public static ProjectFileMock Script(IProject project, string location)
        {
            return new ProjectFileMock(project, "Script", "cs", location);
        }
    }
}