using System.ComponentModel;

namespace Scrawlbit.Notification.State
{
    public interface IChangesState : INotifyPropertyChanged
    {
        bool HasChanges { get; }
    }

    public interface IChangesStateNotifier : IChangesState
    {
        void NotifyChange();
    }

    public interface IChangesStateManager : IChangesStateNotifier
    {
        void Clear();
    }
}