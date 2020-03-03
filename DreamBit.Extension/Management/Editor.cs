using DreamBit.Game.Elements;
using Scrawlbit.Notification;
using System.ComponentModel;

namespace DreamBit.Extension.Management
{
    public interface IEditor : INotifyPropertyChanged
    {
        Scene OpenedScene { get; set; }
    }

    public class Editor : NotificationObject, IEditor
    {
        private Scene _openedScene;

        public Scene OpenedScene
        {
            get => _openedScene;
            set => Set(ref _openedScene, value);
        }
    }
}
