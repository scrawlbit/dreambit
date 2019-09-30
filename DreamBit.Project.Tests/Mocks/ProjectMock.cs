using System.Collections.Generic;
using DreamBit.Project.Registrations;
using Scrawlbit.Helpers;

namespace DreamBit.Project.Mocks
{
    public class ProjectMock : IProject
    {
        public ProjectMock()
        {
            Files = new List<IProjectFile>();
            Registrations = new List<IProjectRegistration>();
        }

        public bool Loaded { get; set; }
        public string Name { get; set; }
        public string Folder { get; set; }
        public string Path { get; set; }
        public List<IProjectFile> Files { get; }
        public List<IProjectRegistration> Registrations { get; }

        IReadOnlyList<IProjectFile> IProject.Files => Files;
        IReadOnlyList<IProjectRegistration> IProject.Registrations => Registrations;

        public void AddRegistration(IProjectRegistration registration)
        {
            Registrations.Add(registration);
        }
        public void AddRegistrations(IProjectRegistrationCollection registration)
        {
            registration.Registrations().ForEach(AddRegistration);
        }

        public IProjectFile AddFile(string path)
        {
            throw new System.NotImplementedException();
        }
        public void IncludeFile(IProjectFile file)
        {
            Files.Add(file);
        }
        public void RenameFile(IProjectFile file, string path)
        {
            throw new System.NotImplementedException();
        }
        public void RemoveFile(IProjectFile file)
        {
            Files.Remove(file);
        }

        public void Load(string path)
        {
            Path = path;
            Loaded = true;
        }
        public void Unload()
        {
            Loaded = false;
        }
        public void Save()
        {
            throw new System.NotImplementedException();
        }
    }
}