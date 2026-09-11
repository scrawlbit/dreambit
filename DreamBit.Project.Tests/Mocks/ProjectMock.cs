using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DreamBit.Project.Mocks
{
    /// <summary>
    /// IProject mínimo para os testes de ProjectFile (só precisa fornecer Folder).
    /// Atualizado para a API atual de IProject.
    /// </summary>
    public class ProjectMock : IProject
    {
        private readonly List<ProjectFile> _files = new List<ProjectFile>();

        public bool Loaded { get; set; }
        public string Name { get; set; }
        public string Folder { get; set; }
        public string Path { get; set; }
        public IReadOnlyList<ProjectFile> Files => _files;

        public event PropertyChangedEventHandler PropertyChanged;

        public T AddFile<T>(string fileName) where T : ProjectFile => throw new NotImplementedException();
        public ProjectFile AddFile(string path) => throw new NotImplementedException();
        public bool MoveFile(string oldPath, string newPath) => throw new NotImplementedException();
        public bool RemoveFile(string path) => throw new NotImplementedException();

        public void Load(string path)
        {
            Path = path;
            Loaded = true;
        }
        public void Unload() => Loaded = false;
        public void Save() => throw new NotImplementedException();
    }
}
