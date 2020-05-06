using Scrawlbit.Notification;
using System;

namespace DreamBit.Game.Elements.Components
{
    public class ScriptProperty : NotificationObject
    {
        private Type _type;
        private object _value;
        private object _defaultValue;
        private Func<Type, object> _jsonValue;

        internal ScriptProperty(Func<Type, object> jsonValue = null)
        {
            _jsonValue = jsonValue;
        }
        
        public string Name { get; internal set; }
        public Type Type
        {
            get => _type;
            internal set => Set(ref _type, value);
        }
        public object Value
        {
            get => _value;
            set => Set(ref _value, value);
        }
        public object DefaultValue
        {
            get => _defaultValue;
            internal set => Set(ref _defaultValue, value);
        }
        internal bool HasJsonValue => _jsonValue != null;

        internal void TrySetJsonValue()
        {
            try
            {
                Value = _jsonValue(Type);
            }
            finally
            {
                _jsonValue = null;
            }
        }
    }
}
