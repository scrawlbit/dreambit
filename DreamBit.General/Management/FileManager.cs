using System.IO;
using System.Text;

namespace DreamBit.Modularization.Management
{
    public interface IFileManager
    {
        bool FileExists(string path);
        bool FolderExists(string path);

        void CreateFolder(string path);

        void Copy(string sourcePath, string destinationPath, bool overwrite);

        void MoveFile(string oldPath, string newPath);
        void MoveFolder(string oldPath, string newPath);

        string ReadAllText(string path);
        void WriteAllText(string path, string contents, Encoding encoding);
    }

    internal class FileManager : IFileManager
    {
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }
        public bool FolderExists(string path)
        {
            return Directory.Exists(path);
        }

        public void CreateFolder(string path)
        {
            Directory.CreateDirectory(path);
        }

        public void Copy(string sourcePath, string destinationPath, bool overwrite)
        {
            File.Copy(sourcePath, destinationPath, overwrite);
        }

        public void MoveFile(string oldPath, string newPath)
        {
            File.Move(oldPath, newPath);
        }
        public void MoveFolder(string oldPath, string newPath)
        {
            Directory.Move(oldPath, newPath);
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }
        public void WriteAllText(string path, string contents, Encoding encoding)
        {
            File.WriteAllText(path, contents, encoding);
        }
    }
}