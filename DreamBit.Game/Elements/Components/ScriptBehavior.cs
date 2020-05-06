using DreamBit.Game.Files;
using Microsoft.Xna.Framework;
using Scrawlbit.Collections;
using Scrawlbit.Helpers;
using System;
using System.Linq;

namespace DreamBit.Game.Elements.Components
{
    public class ScriptBehavior : GameComponent
    {
        private readonly IObservableCollection<ScriptProperty> _properties;
        private IScriptFile _file;
        private string _name;

        public ScriptBehavior(IScriptFile file = null)
        {
            _properties = new ExtendedObservableCollection<ScriptProperty>();

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
        public IReadOnlyObservableCollection<ScriptProperty> Properties => _properties;

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
                ScriptProperty property = _properties.SingleOrDefault(p => p.Name == name) ?? new ScriptProperty();
                (Type type, object defaultValue) = DetermineType(typeName);

                if (type != property.Type)
                    property.Value = defaultValue;

                property.Name = name;
                property.Type = type;
                property.DefaultValue = defaultValue;

                if (property.Type == null)
                {
                    _properties.Remove(property);
                }
                else if (!_properties.Contains(property))
                {
                    _properties.Add(property);
                }
            }
        }
        public void SetValue(string propertyName, Type propertyType, object value)
        {
            ScriptProperty property = Properties.SingleOrDefault(p => p.Name == propertyName && p.Type == propertyType);

            if (property != null)
                property.Value = value;
        }

        private static (Type Type, object DefaultValue) DetermineType(string name)
        {
            (Type Type, object DefaultValue) Value<T>() => (typeof(T), default(T));

            switch (name)
            {
                case "int": return Value<int>();
                case "bool": return Value<bool>();
                case "string": return Value<string>();
                case "float": return Value<float>();
                case "Vector2": return Value<Vector2>();

                default: return (null, null);
            }
        }
    }
}
