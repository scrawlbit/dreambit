using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using DreamBit.Project.Registrations;

namespace DreamBit.Project.Serialization.Converters
{
    internal class ProjectItemConverter : JsonConverter
    {
        private readonly IProject _project;
        private readonly IProjectRegistration[] _registrations;

        public ProjectItemConverter(IProject project)
        {
            _project = project;
            _registrations = project.Registrations.ToArray();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var type = jObject[nameof(IProjectFile.Type)].ToString();
            var id = jObject[nameof(IProjectFile.Id)].ToObject<Guid>();
            var location = jObject[nameof(IProjectFile.Location)].ToString();

            var factory = _registrations.Single(f => f.Type == type);
            var instance = factory.CreateInstance();

            instance.Project = _project;
            instance.Id = id;
            instance.Location = location;

            return instance;
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var jObject = new JObject();
            var file = (IProjectFile)value;

            jObject[nameof(file.Type)] = file.Type;
            jObject[nameof(file.Location)] = file.Location;
            jObject[nameof(file.Id)] = file.Id;

            jObject.WriteTo(writer);
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IProjectFile).IsAssignableFrom(objectType);
        }
    }
}