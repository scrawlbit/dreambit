using Microsoft.Xna.Framework;
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

        internal ScriptProperty(string name, Func<Type, object> jsonValue = null)
        {
            _jsonValue = jsonValue;

            Name = name;
        }

        public string Name { get; }
        public Type Type
        {
            get => _type;
            private set => Set(ref _type, value);
        }
        public object DefaultValue
        {
            get => _defaultValue;
            private set => Set(ref _defaultValue, value);
        }
        public object Value
        {
            get => _value;
            set => Set(ref _value, value);
        }
        internal bool HasJsonValue => _jsonValue != null;

        internal void SetType(string name)
        {
            switch (name)
            {
                case "int": SetType<int>(); break;
                case "bool": SetType<bool>(); break;
                case "string": SetType<string>(); break;
                case "float": SetType<float>(); break;
                case "Vector2": SetType<Vector2>(); break;

                default: SetType(null, null); break;
            }
        }
        internal void TrySetJsonValue()
        {
            try
            {
                if (Type != null)
                    Value = _jsonValue(Type);
            }
            finally
            {
                _jsonValue = null;
            }
        }

        private void SetType<T>()
        {
            SetType(typeof(T), default(T));
        }
        private void SetType(Type type, object defaultValue)
        {
            bool changed = type != Type;

            Type = type;
            DefaultValue = defaultValue;

            if (changed)
                TryChangeValueType();
        }
        private void TryChangeValueType()
        {
            try
            {
                if (Type != null)
                    Value = Convert.ChangeType(Value, Type);
            }
            catch
            {
                Value = DefaultValue;
            }
        }
    }
}
