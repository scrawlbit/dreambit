using System;

namespace DreamBit.Extension.ViewModels.Dialogs
{
    public abstract class BaseDialogViewModel : BaseViewModel
    {
        public event Action Closed;

        protected virtual void Close()
        {
            Closed?.Invoke();
        }
    }
}
