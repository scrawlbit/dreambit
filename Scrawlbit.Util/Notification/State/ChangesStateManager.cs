namespace Scrawlbit.Notification.State
{
    public class ChangesStateManager : NotificationObject, IChangesStateManager
    {
        private bool _hasChanges;

        public bool HasChanges
        {
            get { return _hasChanges; }
            private set
            {
                if (value == _hasChanges) return;
                _hasChanges = value;
                OnPropertyChanged();
            }
        }

        public void NotifyChange()
        {
            HasChanges = true;
        }
        public void Clear()
        {
            HasChanges = false;
        }
    }
}