using DreamBit.Project.Helpers;
using DreamBit.Project.Registrations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace DreamBit.Project.Serialization.Converters
{
    internal class ProjectItemConverter : JsonConverter
    {
        private readonly IProject _project;
        private readonly IProjectManager _manager;

        public ProjectItemConverter(IProject project, IProjectManager manager)
        {
            _project = project;
            _manager = manager;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            Guid id = jObject[nameof(IProjectFile.Id)].ToObject<Guid>();
            string type = jObject[nameof(IProjectFile.Type)].ToString();
            string location = jObject[nameof(IProjectFile.Location)].ToString();

            IFileRegistration factory = _manager.Registrations.DetermineFromType(type);
            ProjectFile instance = factory.CreateInstance();

            instance.Project = _project;
            instance.Id = id;
            instance.Location = location;

            return instance;
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JObject jObject = new JObject();
            IProjectFile file = (IProjectFile)value;

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