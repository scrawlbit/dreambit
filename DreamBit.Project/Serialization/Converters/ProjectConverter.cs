using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scrawlbit.Helpers;

namespace DreamBit.Project.Serialization.Converters
{
    internal class ProjectConverter : JsonConverter
    {
        private readonly IProject _project;
        private readonly IProjectManager _manager;

        public ProjectConverter(IProject project, IProjectManager manager)
        {
            _project = project;
            _manager = manager;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var files = jObject[nameof(_project.Files)].ToObject<ProjectFile[]>(serializer);

            foreach (var file in files)
                _manager.IncludeFile(file);

            return _project;
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var jObject = new JObject();
            var project = (IProject)value;

            jObject[nameof(project.Files)] = JArray.FromObject(project.Files, serializer);

            jObject.WriteTo(writer);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsAssignableTo<IProject>();
        }
    }
}