using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace DreamBit.Extension.Components
{
    public interface ISolutionMonitor
    {
        event Action<string> SolutionOpened;
        event Action SolutionClosed;
        event Action<string[]> ItemsAdded;
        event Action<string[], string[]> ItemsMoved;
        event Action<string[]> ItemsRemoved;
    }

    internal class SolutionMonitor : ISolutionMonitor, IVsSolutionEvents, IVsTrackProjectDocumentsEvents2, IDisposable
    {
        private readonly IPackageBridge _package;
        private IVsSolution _solution;
        private IVsTrackProjectDocuments2 _track;
        private uint _solutionEventsCookie;
        private uint _trackEventsCookie;

        public SolutionMonitor(IPackageBridge package)
        {
            _package = package;
        }

        public event Action<string> SolutionOpened;
        public event Action SolutionClosed;
        public event Action<string[]> ItemsAdded;
        public event Action<string[], string[]> ItemsMoved;
        public event Action<string[]> ItemsRemoved;

        public async Task InitializeAsync()
        {
            await SubscribeToEventsAsync();
            NotifyOpenedSolution();
        }

        // solutions
        int IVsSolutionEvents.OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            return VSConstants.S_OK;
        }
        int IVsSolutionEvents.OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }
        int IVsSolutionEvents.OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            return VSConstants.S_OK;
        }
        int IVsSolutionEvents.OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }
        int IVsSolutionEvents.OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }
        int IVsSolutionEvents.OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }
        int IVsSolutionEvents.OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            SolutionOpened?.Invoke(_package.GetSolutionDirectory());

            return VSConstants.S_OK;
        }
        int IVsSolutionEvents.OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }
        int IVsSolutionEvents.OnBeforeCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }
        int IVsSolutionEvents.OnAfterCloseSolution(object pUnkReserved)
        {
            SolutionClosed?.Invoke();

            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryAddFiles(IVsProject pProject, int cFiles, string[] rgpszMkDocuments, VSQUERYADDFILEFLAGS[] rgFlags, VSQUERYADDFILERESULTS[] pSummaryResult, VSQUERYADDFILERESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }
        int IVsTrackProjectDocumentsEvents2.OnAfterAddFilesEx(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSADDFILEFLAGS[] rgFlags)
        {
            ItemsAdded?.Invoke(rgpszMkDocuments);

            return VSConstants.S_OK;
        }
        int IVsTrackProjectDocumentsEvents2.OnAfterAddDirectoriesEx(int cProjects, int cDirectories, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSADDDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.S_OK;
        }
        int IVsTrackProjectDocumentsEvents2.OnAfterRemoveFiles(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSREMOVEFILEFLAGS[] rgFlags)
        {
            ItemsRemoved?.Invoke(rgpszMkDocuments);

            return VSConstants.S_OK;
        }
        int IVsTrackProjectDocumentsEvents2.OnAfterRemoveDirectories(int cProjects, int cDirectories, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSREMOVEDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.S_OK;
        }
        int IVsTrackProjectDocumentsEvents2.OnQueryRenameFiles(IVsProject pProject, int cFiles, string[] rgszMkOldNames, string[] rgszMkNewNames, VSQUERYRENAMEFILEFLAGS[] rgFlags, VSQUERYRENAMEFILERESULTS[] pSummaryResult, VSQUERYRENAMEFILERESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }
        int IVsTrackProjectDocumentsEvents2.OnAfterRenameFiles(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgszMkOldNames, string[] rgszMkNewNames, VSRENAMEFILEFLAGS[] rgFlags)
        {
            ItemsMoved?.Invoke(rgszMkOldNames, rgszMkNewNames);

            return VSConstants.S_OK;
        }
        int IVsTrackProjectDocumentsEvents2.OnQueryRenameDirectories(IVsProject pProject, int cDirs, string[] rgszMkOldNames, string[] rgszMkNewNames, VSQUERYRENAMEDIRECTORYFLAGS[] rgFlags, VSQUERYRENAMEDIRECTORYRESULTS[] pSummaryResult, VSQUERYRENAMEDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }
        int IVsTrackProjectDocumentsEvents2.OnAfterRenameDirectories(int cProjects, int cDirs, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgszMkOldNames, string[] rgszMkNewNames, VSRENAMEDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.S_OK;
        }
        int IVsTrackProjectDocumentsEvents2.OnQueryAddDirectories(IVsProject pProject, int cDirectories, string[] rgpszMkDocuments, VSQUERYADDDIRECTORYFLAGS[] rgFlags, VSQUERYADDDIRECTORYRESULTS[] pSummaryResult, VSQUERYADDDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }
        int IVsTrackProjectDocumentsEvents2.OnQueryRemoveFiles(IVsProject pProject, int cFiles, string[] rgpszMkDocuments, VSQUERYREMOVEFILEFLAGS[] rgFlags, VSQUERYREMOVEFILERESULTS[] pSummaryResult, VSQUERYREMOVEFILERESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }
        int IVsTrackProjectDocumentsEvents2.OnQueryRemoveDirectories(IVsProject pProject, int cDirectories, string[] rgpszMkDocuments, VSQUERYREMOVEDIRECTORYFLAGS[] rgFlags, VSQUERYREMOVEDIRECTORYRESULTS[] pSummaryResult, VSQUERYREMOVEDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }
        int IVsTrackProjectDocumentsEvents2.OnAfterSccStatusChanged(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, uint[] rgdwSccStatus)
        {
            return VSConstants.S_OK;
        }

        private async Task SubscribeToEventsAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            _solution = await _package.GetServiceAsync<IVsSolution, SVsSolution>();
            _track = await _package.GetServiceAsync<IVsTrackProjectDocuments2, SVsTrackProjectDocuments>();

            _solution.AdviseSolutionEvents(this, out _solutionEventsCookie);
            _track.AdviseTrackProjectDocumentsEvents(this, out _trackEventsCookie);
        }
        private void NotifyOpenedSolution()
        {
            var path = _package.GetSolutionDirectory();
            if (path != null)
                SolutionOpened?.Invoke(path);
        }

        private void UnadviceSolutionEvents()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (_solution != null)
                ErrorHandler.ThrowOnFailure(_solution.UnadviseSolutionEvents(_solutionEventsCookie));
        }
        private void UnadviceTrackEvents()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (_track != null)
                ErrorHandler.ThrowOnFailure(_track.UnadviseTrackProjectDocumentsEvents(_trackEventsCookie));
        }

        public void Dispose()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            GC.SuppressFinalize(this);

            UnadviceSolutionEvents();
            UnadviceTrackEvents();
        }
    }
}