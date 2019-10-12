using DreamBit.Extension.Helpers;
using DreamBit.Modularization.Management;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Scrawlbit.Helpers;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace DreamBit.Extension.Components
{
    public interface IPackageBridge
    {
        CancellationToken DisposalToken { get; }
        ISolutionMonitor Monitor { get; }

        Task InitializeAsync();
        Task SwitchToMainThreadAsync();
        Task<T> GetServiceAsync<T>();
        Task<TCast> GetServiceAsync<TCast, TService>();

        string GetSolutionDirectory();
        string[] GetLoadedProjects();
        bool IsValidProjectPath(string path);
        bool IsSingleHierarchySelected(out IHierarchyBridge hierarchy);
        bool IsSingleItemSelected(out IHierarchyBridge hierarchy);

        void ShowWindow<T>() where T : ToolWindow;
    }

    public class PackageBridge : IPackageBridge
    {
        private readonly AsyncPackage _package;
        private readonly IFileManager _fileManager;
        private readonly SolutionMonitor _monitor;
        private IVsMonitorSelection _selectionMonitor;
        private IVsSolution _solution;

        public PackageBridge(AsyncPackage package, IFileManager fileManager)
        {
            _package = package;
            _fileManager = fileManager;
            _monitor = new SolutionMonitor(this);
        }

        public CancellationToken DisposalToken => _package.DisposalToken;
        public ISolutionMonitor Monitor => _monitor;

        public async Task InitializeAsync()
        {
            _selectionMonitor = await GetServiceAsync<IVsMonitorSelection, SVsShellMonitorSelection>();
            _solution = await GetServiceAsync<IVsSolution, SVsSolution>();

            await _monitor.InitializeAsync();
        }
        public async Task SwitchToMainThreadAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(DisposalToken);
        }
        public Task<T> GetServiceAsync<T>()
        {
            return GetServiceAsync<T, T>();
        }
        public async Task<TCast> GetServiceAsync<TCast, TService>()
        {
            return (TCast)await _package.GetServiceAsync(typeof(TService));
        }

        public string GetSolutionDirectory()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _solution.GetSolutionInfo(out var directory, out _, out _).ThrowOnFailure();

            return directory;
        }
        public string[] GetLoadedProjects()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            const uint skipOption = (uint)__VSGETPROJFILESFLAGS.GPFF_SKIPUNLOADEDPROJECTS;
            uint projectsCount;

            _solution.GetProjectFilesInSolution(skipOption, 0, null, out projectsCount).ThrowOnFailure();

            var projects = new string[projectsCount];
            _solution.GetProjectFilesInSolution(skipOption, 0, projects, out projectsCount).ThrowOnFailure();

            return projects.Where(IsValidProjectPath).ToArray();
        }
        public bool IsValidProjectPath(string path)
        {
            if (!path.HasValue())
                return false;

            var extensions = new[] { "shproj", "csproj" };
            return extensions.Any(path.EndsWith);
        }
        public bool IsSingleHierarchySelected(out IHierarchyBridge hierarchy)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            IntPtr hierarchyPointer = IntPtr.Zero;
            IntPtr selectionPointer = IntPtr.Zero;

            try
            {
                if (IsSingleHierarchySelected(out hierarchyPointer, out selectionPointer, out uint itemId))
                {
                    var vsHierarchy = (IVsHierarchy)Marshal.GetObjectForIUnknown(hierarchyPointer);
                    hierarchy = new HierarchyBridge(vsHierarchy, itemId, _fileManager);
                    return true;
                }
            }
            finally
            {
                selectionPointer.Release();
                hierarchyPointer.Release();
            }

            hierarchy = null;
            return false;
        }
        public bool IsSingleItemSelected(out IHierarchyBridge hierarchy)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (IsSingleItemSelected(out IVsHierarchy vsHierarchy, out uint itemId))
            {
                hierarchy = new HierarchyBridge(vsHierarchy, itemId, _fileManager);
                return true;
            }

            hierarchy = null;
            return false;
        }

        public void ShowWindow<T>() where T : ToolWindow
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var window = _package.FindToolWindow(typeof(T), 0, true);
            var windowFrame = (IVsWindowFrame)window?.Frame;

            if (windowFrame == null)
                throw new NotSupportedException($"Cannot create {typeof(T).Name}");

            ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }
        public void AddItem(IVsHierarchy hierarchy, string path)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            IntPtr handleDialogPointer = IntPtr.Zero;

            try
            {
                const uint to = VSConstants.VSITEMID_ROOT;
                const VSADDITEMOPERATION operation = VSADDITEMOPERATION.VSADDITEMOP_OPENFILE;
                const int numberOfItensToOpen = 1;
                string[] itensToOpen = new[] { path };
                VSADDRESULT[] results = new VSADDRESULT[1];

                ((IVsProject4)hierarchy).AddItem(to, operation, path, numberOfItensToOpen, itensToOpen, handleDialogPointer, results);
            }
            finally
            {
                handleDialogPointer.Release();
            }
        }

        private bool IsSingleHierarchySelected(out IntPtr hierarchyPointer, out IntPtr selectionPointer, out uint itemId)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            hierarchyPointer = IntPtr.Zero;
            selectionPointer = IntPtr.Zero;
            itemId = VSConstants.VSITEMID_NIL;

            var currentHierarchy = _selectionMonitor.GetCurrentSelection(
                out hierarchyPointer,
                out itemId,
                out var multiItemSelect,
                out selectionPointer
            );

            // there is no selection
            if (ErrorHandler.Failed(currentHierarchy) || hierarchyPointer == IntPtr.Zero || itemId == VSConstants.VSITEMID_NIL)
                return false;

            // multiple items are selected
            if (multiItemSelect != null)
                return false;

            return true;
        }
        private bool IsSingleItemSelected(out IVsHierarchy hierarchy, out uint itemId)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            hierarchy = null;
            var hierarchyPointer = IntPtr.Zero;
            var selectionPointer = IntPtr.Zero;

            try
            {
                if (!IsSingleHierarchySelected(out hierarchyPointer, out selectionPointer, out itemId))
                    return false;

                // there is a hierarchy root node selected, thus it is not a single item inside a project
                if (itemId == VSConstants.VSITEMID_ROOT)
                    return false;

                if ((hierarchy = Marshal.GetObjectForIUnknown(hierarchyPointer) as IVsHierarchy) == null)
                    return false;

                // hierarchy is not a project inside the Solution if it does not have a ProjectID Guid
                if (ErrorHandler.Failed(_solution.GetGuidOfProject(hierarchy, out _)))
                    return false;

                // if we got this far then there is a single project item selected
                return true;
            }
            finally
            {
                selectionPointer.Release();
                hierarchyPointer.Release();
            }
        }
    }
}