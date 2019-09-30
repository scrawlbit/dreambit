using DreamBit.Project.Serialization;

namespace DreamBit.Project.Mocks
{
    public class SerializerMock : ISerializer
    {
        public IProject Project { get; set; }
        public bool Loaded { get; set; }
        public bool Saved { get; set; }

        public void Load(IProject project)
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