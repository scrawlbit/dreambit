using DreamBit.Game.Files;
using DreamBit.Modularization.Management;
using DreamBit.Project;
using DreamBit.Project.Registrations;
using System;
using System.Text.RegularExpressions;

namespace DreamBit.Game.Registrations
{
    internal interface IScriptFileRegistration : IFileRegistration { }
    internal class ScriptFileRegistration : IScriptFileRegistration
    {
        private readonly IFileManager _fileManager;
        private readonly IProject _project;

        public ScriptFileRegistration(IFileManager fileManager, IProject project)
        {
            _fileManager = fileManager;
            _project = project;
        }

        public string Type => "Script";
        public string Extension => ".cs";
        public Type ObjectType => typeof(ScriptFile);

        public bool ShouldIncludeFromExternalAction(string path)
        {
            string content = _fileManager.ReadAllText(path);

            return Regex.IsMatch(content, @"class (\w+) ?:.+ScriptBehavior");
        }
        public ProjectFile CreateInstance() => new ScriptFile(_fileManager, _project);
    }
}
