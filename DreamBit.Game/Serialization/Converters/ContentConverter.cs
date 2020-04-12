using DreamBit.Game.Content;
using DreamBit.Project;
using DreamBit.Project.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace DreamBit.Game.Serialization.Converters
{
    internal class ContentConverter : JsonConverter<IContent>
    {
        private readonly IProject _project;
        private readonly IContentFactory _factory;

        public ContentConverter(IProject project, IContentFactory factory)
        {
            _project = project;
            _factory = factory;
        }

        public override IContent ReadJson(JsonReader reader, Type objectType, IContent existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            Guid? id = token.ToObject<Guid?>();

            if (id != null)
            {
                ProjectFile file = _project.Files.GetById(id.Value);

                if (file != null)
                    return _factory.Create(file);
            }

            return null;
        }
        public override void WriteJson(JsonWriter writer, IContent value, JsonSerializer serializer)
        {
            JToken token = JToken.FromObject(value?.File.Id, serializer);

            token.WriteTo(writer);
        }
    }
}
