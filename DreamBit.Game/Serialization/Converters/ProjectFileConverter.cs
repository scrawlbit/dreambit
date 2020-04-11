using DreamBit.Project;
using DreamBit.Project.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace DreamBit.Game.Serialization.Converters
{
    internal class ProjectFileConverter : JsonConverter<ProjectFile>
    {
        private readonly IProject _project;

        public ProjectFileConverter(IProject project)
        {
            _project = project;
        }

        public override ProjectFile ReadJson(JsonReader reader, Type objectType, ProjectFile existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            Guid? id = token.ToObject<Guid?>();

            if (id != null)
                return _project.Files.GetById(id.Value);

            return null;
        }
        public override void WriteJson(JsonWriter writer, ProjectFile value, JsonSerializer serializer)
        {
            JToken token = JToken.FromObject(value?.Id, serializer);

            token.WriteTo(writer);
        }
    }
}
