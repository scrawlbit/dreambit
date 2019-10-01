using Scrawlbit.Notification;

namespace DreamBit.Extension.Models
{
    public interface ISceneObject
    {
        bool IsExpanded { get; set; }
        bool IsSelected { get; set; }
        string Name { get; set; }
        ISceneObject Parent { get; set; }
        ISceneObjectCollection Children { get; }
    }

    public class SceneObject : NotificationObject, ISceneObject
    {
        private bool _isExpanded;
        private bool _isSelected;
        private string _name;
        private ISceneObject _parent;

        public SceneObject()
        {
            Children = new SceneObjectCollection(this);
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set => Set(ref _isExpanded, value);
        }
        public bool IsSelected
        {
            get => _isSelected;
            set => Set(ref _isSelected, value);
        }
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }
        public ISceneObject Parent
        {
            get => _parent;
            set => Set(ref _parent, value);
        }
        public ISceneObjectCollection Children { get; }
    }
}