using System;
using System.IO;
using System.Linq;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Project;
using DreamBit.Engine.Serialization;
using Xunit;

namespace DreamBit.Engine.Tests
{
    public class ProjectTests : IDisposable
    {
        private string _folder = null!;

        public ProjectTests()
        {
            _folder = Path.Combine(Path.GetTempPath(), "dreambit_proj_" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_folder);
        }

        public void Dispose()
        {
            if (Directory.Exists(_folder))
                Directory.Delete(_folder, true);
        }

        [Fact]
        public void CreateOrOpen_GravaOArquivoDeProjeto()
        {
            var project = GameProject.CreateOrOpen(_folder, "MeuJogo");

            Assert.True(File.Exists(project.ProjectFilePath));
            Assert.Equal("MeuJogo", project.Name);
        }

        [Fact]
        public void EnumerateScenes_ListaOsArquivosDbscene()
        {
            var project = GameProject.CreateOrOpen(_folder, "Jogo");
            SceneSerializer.Save(new Scene { Name = "Fase1" }, project.ScenePath("Fase1.dbscene"));
            SceneSerializer.Save(new Scene { Name = "Fase2" }, project.ScenePath("Fase2.dbscene"));

            var scenes = project.EnumerateScenes().ToList();

            CollectionAssert.AreEquivalent(new[] { "Fase1.dbscene", "Fase2.dbscene" }, scenes);
        }

        [Fact]
        public void EnumerateScenes_IncluiSubpastas_EIgnoraBin()
        {
            var project = GameProject.CreateOrOpen(_folder, "Jogo");
            Directory.CreateDirectory(Path.Combine(_folder, "scenes"));
            Directory.CreateDirectory(Path.Combine(_folder, "bin"));
            SceneSerializer.Save(new Scene { Name = "Cela" }, Path.Combine(_folder, "scenes", "01_cela.dbscene"));
            SceneSerializer.Save(new Scene { Name = "Lixo" }, Path.Combine(_folder, "bin", "copia.dbscene"));

            var scenes = project.EnumerateScenes().ToList();

            Assert.Contains("scenes/01_cela.dbscene", scenes);
            Assert.DoesNotContain(scenes, s => s.StartsWith("bin/"));
            Assert.True(File.Exists(project.ScenePath("scenes/01_cela.dbscene")));
        }

        [Fact]
        public void Load_PreservaONome()
        {
            var created = GameProject.CreateOrOpen(_folder, "Projeto X");
            var loaded = GameProject.Load(created.ProjectFilePath);

            Assert.Equal("Projeto X", loaded.Name);
            Assert.Equal(_folder, loaded.Folder);
        }
    }
}
