using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ScrawlBit.Helpers;

namespace DreamBit.Project.Serialization.Converters
{
    internal class ProjectConverter : JsonConverter
    {
        private readonly IProject _project;

        public ProjectConverter(IProject project)
        {
            _project = project;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var files = jObject[nameof(_project.Files)].ToObject<IProjectFile[]>(serializer);

            foreach (var file in files)
                _project.IncludeFile(file);

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