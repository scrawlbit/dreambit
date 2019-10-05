using System.Text;
using DreamBit.Modularization.Management;
using DreamBit.Project.Serialization.Converters;
using Newtonsoft.Json.Converters;
using Scrawlbit.Json;

namespace DreamBit.Project.Serialization
{
    internal interface ISerializer
    {
        void Load(IProject project, IProjectManager manager);
        void Save(IProject project);
    }

    internal class Serializer : ISerializer
    {
        private readonly IJsonParser _jsonParser;
        private readonly IFileManager _fileManager;
        private readonly UTF8Encoding _encoding;
        private bool _convertersDefined;

        public Serializer(IJsonParser jsonParser, IFileManager fileManager)
        {
            _jsonParser = jsonParser;
            _fileManager = fileManager;
            _encoding = new UTF8Encoding(false);
        }

        public void Load(IProject project, IProjectManager manager)
        {
            if (!_convertersDefined)
            {
                _jsonParser.Converters.Add(new StringEnumConverter());
                _jsonParser.Converters.Add(new ProjectConverter(project, manager));
                _jsonParser.Converters.Add(new ProjectItemConverter(project, manager));

                _convertersDefined = true;
            }

            var json = _fileManager.ReadAllText(project.Path);

            _jsonParser.ParseString<IProject>(json);
        }
        public void Save(IProject project)
        {
            var json = _jsonParser.ParseObject(project);

            _fileManager.WriteAllText(project.Path, json, _encoding);
        }
    }
}