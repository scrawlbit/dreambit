using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace DreamBit.Engine.Project
{
    /// <summary>
    /// Projeto DreamBit: uma pasta com cenas (.dbscene). O arquivo .dbproj guarda o
    /// nome; as cenas são descobertas varrendo a pasta (mantidas vivas pelo
    /// ProjectWatcher). Equivale, de forma enxuta, ao papel de DreamBit.Project.
    /// </summary>
    public sealed class GameProject
    {
        public const string ProjectExtension = ".dbproj";
        public const string SceneExtension = ".dbscene";

        private GameProject(string name, string folder)
        {
            Name = name;
            Folder = folder;
        }

        public string Name { get; set; }
        public string Folder { get; }

        /// <summary>Caminho do arquivo .dbproj.</summary>
        public string ProjectFilePath => Path.Combine(Folder, Name + ProjectExtension);

        /// <summary>Cenas (nomes de arquivo) presentes na pasta do projeto.</summary>
        public IEnumerable<string> EnumerateScenes()
        {
            if (!Directory.Exists(Folder))
                return Enumerable.Empty<string>();

            return Directory
                .EnumerateFiles(Folder, "*" + SceneExtension, SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName)
                .Where(name => name != null)
                .Select(name => name!)
                .OrderBy(name => name);
        }

        public string ScenePath(string sceneFileName) => Path.Combine(Folder, sceneFileName);

        private static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg", ".bmp" };

        /// <summary>Caminhos completos das imagens (assets) na pasta do projeto (recursivo).</summary>
        public IEnumerable<string> EnumerateAssets()
        {
            if (!Directory.Exists(Folder))
                return Enumerable.Empty<string>();

            return Directory
                .EnumerateFiles(Folder, "*.*", SearchOption.AllDirectories)
                .Where(f => ImageExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .OrderBy(f => f);
        }

        /// <summary>Caminhos das imagens relativos à pasta do projeto.</summary>
        public IEnumerable<string> EnumerateAssetRelativePaths()
            => EnumerateAssets().Select(a => Path.GetRelativePath(Folder, a));

        /// <summary>Gera/atualiza o Content.mgcb do projeto e retorna seu caminho.</summary>
        public string WriteContentManifest(string platform = "DesktopGL")
        {
            var text = ContentManifest.Generate(EnumerateAssetRelativePaths(), platform);
            var path = Path.Combine(Folder, ContentManifest.FileName);
            File.WriteAllText(path, text);
            return path;
        }

        public void Save()
        {
            var data = new ProjectData { Name = Name };
            File.WriteAllText(ProjectFilePath, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
        }

        /// <summary>Abre um projeto a partir do arquivo .dbproj.</summary>
        public static GameProject Load(string projectFilePath)
        {
            var data = JsonSerializer.Deserialize<ProjectData>(File.ReadAllText(projectFilePath)) ?? new ProjectData();
            var folder = Path.GetDirectoryName(projectFilePath) ?? Directory.GetCurrentDirectory();
            var name = string.IsNullOrWhiteSpace(data.Name)
                ? Path.GetFileNameWithoutExtension(projectFilePath)
                : data.Name;
            return new GameProject(name, folder);
        }

        /// <summary>Cria (ou reutiliza) um projeto numa pasta, gravando o .dbproj.</summary>
        public static GameProject CreateOrOpen(string folder, string name)
        {
            var project = new GameProject(name, folder);
            if (!File.Exists(project.ProjectFilePath))
                project.Save();
            return project;
        }

        private sealed class ProjectData
        {
            public string Name { get; set; } = "Projeto";
        }
    }
}
