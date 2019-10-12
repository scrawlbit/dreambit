using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;

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

        bool IsSingleHierarchySelected(out IHierarchyBridge hierarchy);
        bool IsSingleItemSelected(out IHierarchyBridge hierarchy);

        string GetSolutionDirectory();
        bool IsSingleHierarchySelected(out IVsHierarchy item);
        bool IsSingleItemSelected(out IVsHierarchy item);
        string GetSelectedItemPath();
        string[] GetLoadedProjects();
        bool IsValidProjectPath(string path);
        string GetProjectFolder(IVsHierarchy item);
        IVsHierarchy GetSingleHierarchySelected();
        string GetHierarchyPath(IVsHierarchy hierarchy);

        void ShowWindow<T>() where T : ToolWindow;
        void AddItem(IVsHierarchy hierarchy, string path);
    }
}