using DreamBit.Game.Files;
using Scrawlbit.Collections;
using Scrawlbit.Helpers;
using System;
using System.Linq;

namespace DreamBit.Game.Elements.Components
{
    public class ScriptBehavior : GameComponent
    {
        private readonly IObservableCollection<Property> _properties;
        private IScriptFile _file;
        private string _name;

        public ScriptBehavior(IScriptFile file = null)
        {
            _properties = new ExtendedObservableCollection<Property>();

            File = file;
        }

        public IScriptFile File
        {
            get => _file;
            set
            {
                if (Set(ref _file, value))
                    UpdateName();
            }
        }
        public override string Name => _name;
        public IReadOnlyObservableCollection<Property> Properties => _properties;

        private void UpdateName()
        {
            _name = File?.Name ?? "Invalid Behavior";
            OnPropertyChanged(nameof(Name));
        }
        public void MergeProperties((string Name, string Type)[] properties)
        {
            var names = properties.Select(p => p.Name).ToArray();
            var removed = _properties.Where(p => !names.Contains(p.Name)).ToArray();

            _properties.RemoveRange(removed);

            foreach (var (name, typeName) in properties)
            {
                Property property = _properties.SingleOrDefault(p => p.Name == name) ?? new Property();
                Type type = DetermineType(typeName);

                if (type != property.Type)
                    property.Value = null;

                property.Name = name;
                property.Type = type;

                if (property.Type == null)
                {
                    _properties.Remove(property);
                }
                else if (!_properties.Contains(property))
                {
                    _properties.InsertOrdered(property, p => p.Name);
                }
            }
        }

        private Type DetermineType(string name)
        {
            switch (name)
            {
                case "int": return typeof(int);
                case "bool": return typeof(bool);
                case "string": return typeof(string);
                case "float": return typeof(float);

                default: return null;
            }
        }

        #region Property

        public class Property
        {
            public string Name { get; internal set; }
            public Type Type { get; internal set; }
            public object Value { get; internal set; }
        }

        #endregion
    }
}
