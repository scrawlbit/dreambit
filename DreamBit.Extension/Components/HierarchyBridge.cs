using DreamBit.Extension.Helpers;
using DreamBit.Modularization.Management;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using _Path = System.IO.Path;

namespace DreamBit.Extension.Components
{
    public interface IHierarchyBridge
    {
        string Path { get; }
        string ProjectFolder { get; }
        string Folder { get; }
        bool IsFolder { get; }

        void AddItem(string path);
    }

    public class HierarchyBridge : IHierarchyBridge
    {
        private readonly IVsHierarchy _hierarchy;
        private readonly uint _itemId;
        private readonly IFileManager _fileManager;
        private string _path;
        private string _projectFolder;
        private string _folder;
        private bool? _isFolder;

        public HierarchyBridge(IVsHierarchy hierarchy, uint itemId, IFileManager fileManager)
        {
            _hierarchy = hierarchy;
            _itemId = itemId;
            _fileManager = fileManager;
        }

        public string Path
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                if (_path == null)
                    ((IVsProject)_hierarchy).GetMkDocument(_itemId, out _path);

                return _path;
            }
        }
        public string ProjectFolder
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                if (_projectFolder == null)
                {
                    ((IVsProject)_hierarchy).GetMkDocument(VSConstants.VSITEMID_ROOT, out var projectPath);
                    _projectFolder = _Path.GetDirectoryName(projectPath);
                }

                return _projectFolder;
            }
        }
        public string Folder
        {
            get => _folder ?? (_folder = _Path.GetDirectoryName(Path));
        }
        public bool IsFolder
        {
            get => _isFolder ?? (_isFolder = _fileManager.FolderExists(Path)).Value;
        }

        public void AddItem(string path)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            IntPtr handleDialogPointer = IntPtr.Zero;

            try
            {
                const VSADDITEMOPERATION operation = VSADDITEMOPERATION.VSADDITEMOP_OPENFILE;
                const int numberOfItensToOpen = 1;
                string[] itensToOpen = new[] { path };
                VSADDRESULT[] results = new VSADDRESULT[1];

                ((IVsProject4)_hierarchy).AddItem(_itemId, operation, path, numberOfItensToOpen, itensToOpen, handleDialogPointer, results);
            }
            finally
            {
                handleDialogPointer.Release();
            }
        }
    }
}
