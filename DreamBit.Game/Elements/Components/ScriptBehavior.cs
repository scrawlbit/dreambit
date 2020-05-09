using DreamBit.Game.Files;
using Microsoft.Xna.Framework;
using Scrawlbit.Collections;
using Scrawlbit.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DreamBit.Game.Elements.Components
{
    public class ScriptBehavior : GameComponent
    {
        private readonly IObservableCollection<ScriptProperty> _properties;
        private IScriptFile _file;
        private string _name;

        public ScriptBehavior(IScriptFile file)
        {
            _properties = new ExtendedObservableCollection<ScriptProperty>();

            File = file;
        }
        internal ScriptBehavior(IScriptFile file, IEnumerable<ScriptProperty> properties) : this(file)
        {
            _properties.AddRange(properties);
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
        public IReadOnlyObservableCollection<ScriptProperty> Properties => _properties;

        public void MergeProperties((string Name, string Type)[] properties)
        {
            var names = properties.Select(p => p.Name).ToArray();
            var removed = _properties.Where(p => !names.Contains(p.Name)).ToArray();

            _properties.RemoveRange(removed);

            foreach (var (name, typeName) in properties)
            {
                ScriptProperty property = _properties.SingleOrDefault(p => p.Name == name) ?? new ScriptProperty(name);

                property.SetType(typeName);

                if (property.HasJsonValue)
                    property.TrySetJsonValue();

                if (property.Type == ScriptPropertyType.Unknown)
                {
                    _properties.Remove(property);
                }
                else if (!_properties.Contains(property))
                {
                    _properties.Add(property);
                }
            }
        }
        public void SetValue(string propertyName, ScriptPropertyType propertyType, object value)
        {
            ScriptProperty property = Properties.SingleOrDefault(p => p.Name == propertyName && p.Type == propertyType);

            if (property != null)
                property.Value = value;
        }

        private void UpdateName()
        {
            _name = File?.Name ?? "Invalid Behavior";
            OnPropertyChanged(nameof(Name));
        }
    }
}
