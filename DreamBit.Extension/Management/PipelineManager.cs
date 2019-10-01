using System.IO;
using System.Linq;
using DreamBit.Extension.Components;
using DreamBit.Pipeline;
using DreamBit.Pipeline.Files;
using DreamBit.Project;

namespace DreamBit.Extension.Management
{
    public interface IPipelineManager
    {
        void Initialize();
    }

    public class PipelineManager : IPipelineManager
    {
        private readonly IPackageBridge _package;
        private readonly IProject _project;
        private readonly IPipeline _pipeline;

        public PipelineManager(IPackageBridge package, IProject project, IPipeline pipeline)
        {
            _package = package;
            _project = project;
            _pipeline = pipeline;
        }

        public void Initialize()
        {
            _package.Monitor.ItemsAdded += OnItemsAdded;
            _package.Monitor.ItemsRemoved += OnItemsRemoved;
        }

        private void OnItemsAdded(string[] paths)
        {
            var updated = false;

            foreach (var path in paths)
            {
                var isContent = path.StartsWith(_project.Folder);
                var extension = Path.GetExtension(path);
                var isImage = extension == $".{PipelineImage.ImageExtension}";

                if (isContent && isImage)
                {
                    updated |= AddImage(path);
                }
            }

            if (updated)
                _pipeline.Save();
        }
        private void OnItemsRemoved(string[] paths)
        {
            var updated = false;

            foreach (var path in paths)
            {
                var isContent = path.StartsWith(_project.Folder);
                if (isContent)
                {
                    updated |= RemoveContent(path);
                }
            }

            if (updated)
                _pipeline.Save();
        }

        private bool AddImage(string path)
        {
            path = GetContentPath(path, out var folder, out var name);

            if (!ImportExists(path))
            {
                var image = new PipelineImage();
                _pipeline.Contents.AddImport(image);

                return true;
            }

            return false;
        }
        private bool RemoveContent(string path)
        {
            path = GetContentPath(path, out _, out _);

            if (ImportExists(path))
            {
                _pipeline.Contents.Remove(path);
                return true;
            }

            return false;
        }

        private string GetContentPath(string path, out string folder, out string name)
        {
            folder = Path.GetDirectoryName(path).Replace($"{_project.Folder}", "");
            name = Path.GetFileName(path);

            if (folder.StartsWith("\\"))
                folder = folder.Substring(1);

            path = Path.Combine(folder, name);
            path = path.Replace('\\', '/');

            return path;
        }
        private bool ImportExists(string path)
        {
            return _pipeline.Contents.Imports.Any(i => i.Path == path);
        }
    }
}