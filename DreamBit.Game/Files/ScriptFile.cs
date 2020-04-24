using DreamBit.Modularization.Management;
using DreamBit.Project;
using System.Text;
using _Path = System.IO.Path;

namespace DreamBit.Game.Files
{
    public interface IScriptFile : IProjectFile { }
    public sealed class ScriptFile : ProjectFile, IScriptFile
    {
        private readonly IFileManager _fileManager;
        private readonly IProject _project;

        internal ScriptFile(IFileManager fileManager, IProject project)
        {
            _fileManager = fileManager;
            _project = project;
        }

        protected override void OnAdded()
        {
            if (_fileManager.FileExists(Path))
                return;

            string solutionFolder = _Path.GetDirectoryName(_project.Folder);
            string solutionName = _Path.GetFileName(solutionFolder);
            string fileFolder = _Path.GetDirectoryName(Location);

            string[] folders = fileFolder.Split('\\');
            string @namespace = $"{solutionName}.{string.Join(".", folders)}";

            string content =
$@"using DreamBit.Game.Elements.Components;

namespace {@namespace}
{{
    public class {Name} : ScriptBehavior
    {{
    }}
}}";

            _fileManager.WriteAllText(Path, content.ToString(), Encoding.UTF8);
        }
    }
}
