using DreamBit.Project.Serialization;

namespace DreamBit.Project.Mocks
{
    internal class SerializerMock : ISerializer
    {
        public IProject Project { get; set; }
        public bool Loaded { get; set; }
        public bool Saved { get; set; }

        public void Load(IProject project, IProjectManager manager)
        {
            Project = project;
            Loaded = true;
        }
        public void Save(IProject project)
        {
            Project = project;
            Saved = true;
        }
    }
}
