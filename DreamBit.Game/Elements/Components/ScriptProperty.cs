using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scrawlbit.Notification;
using System;

namespace DreamBit.Game.Elements.Components
{
    public class ScriptProperty : NotificationObject
    {
        private ScriptPropertyType _type;
        private Type _valueType;
        private object _value;
        private object _defaultValue;
        private JsonSerializer _jsonSerializer;

        internal ScriptProperty(string name)
        {
            Name = name;
        }
        internal ScriptProperty(string name, JToken jsonToken, JsonSerializer jsonSerializer) : this(name)
        {
            _jsonSerializer = jsonSerializer;

            JsonToken = jsonToken;
        }

        public string Name { get; }
        public ScriptPropertyType Type
        {
            get => _type;
            set => Set(ref _type, value);
        }
        public Type ValueType
        {
            get => _valueType;
            private set => Set(ref _valueType, value);
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
        internal JToken JsonToken { get; private set; }
        internal bool HasJsonValue => JsonToken != null;

        internal void SetType(string name)
        {
            switch (name)
            {
                case "int": SetType<int>(ScriptPropertyType.Int); break;
                case "bool": SetType<bool>(ScriptPropertyType.Bool); break;
                case "string": SetType<string>(ScriptPropertyType.String); break;
                case "float": SetType<float>(ScriptPropertyType.Float); break;
                case "Vector2": SetType<Vector2>(ScriptPropertyType.Vector2); break;
                case "GameObject": SetType<Guid>(ScriptPropertyType.GameObject); break;

                default: SetType(ScriptPropertyType.Unknown, null, null); break;
            }
        }
        internal void TrySetJsonValue()
        {
            try
            {
                if (Type != ScriptPropertyType.Unknown)
                    Value = JsonToken.ToObject(ValueType, _jsonSerializer);
            }
            finally
            {
                _jsonSerializer = null;
                JsonToken = null;
            }
        }

        private void SetType<T>(ScriptPropertyType type)
        {
            SetType(type, typeof(T), default(T));
        }
        private void SetType(ScriptPropertyType type, Type valueType, object defaultValue)
        {
            bool changed = type != Type;

            Type = type;
            ValueType = valueType;
            DefaultValue = defaultValue;

            if (changed)
                TryChangeValueType();
        }
        private void TryChangeValueType()
        {
            try
            {
                if (Type != ScriptPropertyType.Unknown)
                    Value = Convert.ChangeType(Value, ValueType);
            }
            catch
            {
                Value = DefaultValue;
            }
        }
    }
}
