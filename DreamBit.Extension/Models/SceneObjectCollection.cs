using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DreamBit.Extension.Models
{
    public interface ISceneObjectCollection : IReadOnlyList<ISceneObject>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        ISceneObject Add(string name);
        bool Remove(ISceneObject sceneObject);
    }

    public class SceneObjectCollection : ObservableCollection<ISceneObject>, ISceneObjectCollection
    {
        private readonly ISceneObject _owner;

        public SceneObjectCollection(ISceneObject owner)
        {
            _owner = owner;
        }

        public ISceneObject Add(string name)
        {
            var sceneObject = new SceneObject
            {
                Name = name,
                Parent = _owner
            };

            Add(sceneObject);

            return sceneObject;
        }
    }
}