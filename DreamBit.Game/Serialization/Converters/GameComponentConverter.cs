using DreamBit.Game.Elements;
using DreamBit.Game.Elements.Components;
using DreamBit.Game.Files;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Scrawlbit.Json;
using System;
using System.Collections.Generic;

namespace DreamBit.Game.Serialization.Converters
{
    internal class GameComponentConverter : JsonConverter<GameComponent>
    {
        private const string Type = "Type";
        private const string Image = "Image";
        private const string Text = "Text";
        private const string Camera = "Camera";
        private const string Script = "Script";

        public override GameComponent ReadJson(JsonReader reader, Type objectType, GameComponent existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);

            if (obj.Property(Script) != null)
                return ReadScriptBehavior(obj, serializer);

            return ReadCommonComponent(obj, serializer);
        }
        public override void WriteJson(JsonWriter writer, GameComponent value, JsonSerializer serializer)
        {
            JObject obj = new JObject();

            if (value is ScriptBehavior script)
                WriteScriptBehavior(obj, script, serializer);
            else
                WriteCommonComponent(obj, value, serializer);

            obj.WriteTo(writer);
        }

        private ScriptBehavior ReadScriptBehavior(JObject obj, JsonSerializer serializer)
        {
            List<ScriptProperty> properties = new List<ScriptProperty>();
            IScriptFile file = obj[Script].ToObject<IScriptFile>(serializer);

            foreach (var p in obj.Properties())
            {
                if (p.Name == Script) continue;

                object Convert(Type type) => p.Value.ToObject(type, serializer);

                properties.Add(new ScriptProperty(Convert) { Name = p.Name });
            }

            return new ScriptBehavior(file, properties);
        }
        private GameComponent ReadCommonComponent(JObject obj, JsonSerializer serializer)
        {
            string typeName = obj[Type].ToString();
            GameComponent component = CreateInstance(typeName);

            serializer.Populate(obj.CreateReader(), component);

            return component;
        }

        private void WriteScriptBehavior(JObject obj, ScriptBehavior value, JsonSerializer serializer)
        {
            obj.SetProperty(Script, value.File, serializer);

            foreach (var property in value.Properties)
            {
                if (!Equals(property.Value, property.DefaultValue))
                    obj.SetProperty(property.Name, property.Value, serializer);
            }
        }
        private void WriteCommonComponent(JObject obj, GameComponent value, JsonSerializer serializer)
        {
            JsonObjectContract contract = (JsonObjectContract)serializer.ContractResolver.ResolveContract(value.GetType());

            obj[Type] = GetTypeName(value);

            foreach (var property in contract.Properties)
            {
                if (property.Readable && property.Writable)
                    obj.SetProperty(property, value, serializer);
            }

            if (value is ScriptBehavior script)
                obj.SetProperty(Script, script.File, serializer);
        }

        private static string GetTypeName(GameComponent value)
        {
            switch (value)
            {
                case ImageRenderer _: return Image;
                case TextRenderer _: return Text;
                case Camera _: return Camera;

                default: throw new NotImplementedException();
            }
        }
        private GameComponent CreateInstance(string typeName)
        {
            switch (typeName)
            {
                case Image: return new ImageRenderer();
                case Text: return new TextRenderer();
                case Camera: return new Camera();

                default: throw new NotImplementedException();
            }
        }
    }
}
