using System.Linq;
using System.Text;
using DreamBit.Modularization.Management;

namespace DreamBit.Project.Mocks
{
    public class FileManagerMock : IFileManager
    {
        public string ExistentFile
        {
            get => ExistentFiles.Single();
            set => ExistentFiles = new[] { value };
        }
        public string ExistentFolder
        {
            get => ExistentFolders.Single();
            set => ExistentFolders = new[] { value };
        }

        public string[] ExistentFiles { get; set; }
        public string[] ExistentFolders { get; set; }
        public string FolderCreated { get; set; }
        public (string SourcePath, string DestinationPath, bool Overwritten) ItemCopied { get; set; }
        public (string OldPath, string NewPath) ItemMoved { get; set; }
        public string TextToRead { get; set; }
        public string TextReadedFrom { get; set; }
        public (string Path, string Text) TextWrote { get; set; }

        public bool FileExists(string path)
        {
            return ExistentFiles?.Contains(path) ?? false;
        }
        public bool FolderExists(string path)
        {
            return ExistentFolders?.Contains(path) ?? false;
        }
        public void CreateFolder(string path)
        {
            FolderCreated = path;
        }
        public void Copy(string sourcePath, string destinationPath, bool overwrite)
        {
            ItemCopied = (sourcePath, destinationPath, overwrite);
        }
        public void MoveFile(string oldPath, string newPath)
        {
            ItemMoved = (oldPath, newPath);
        }
        public void MoveFolder(string oldPath, string newPath)
        {
            ItemMoved = (oldPath, newPath);
        }
        public string ReadAllText(string path)
        {
            TextReadedFrom = path;

            return TextToRead;
        }
        public void WriteAllText(string path, string contents, Encoding encoding)
        {
            TextWrote = (path, contents);
        }
    }
}