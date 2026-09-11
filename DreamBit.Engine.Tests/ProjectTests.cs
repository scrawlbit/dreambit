using System.IO;
using System.Linq;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Project;
using DreamBit.Engine.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class ProjectTests
    {
        private string _folder = null!;

        [TestInitialize]
        public void Setup()
        {
            _folder = Path.Combine(Path.GetTempPath(), "dreambit_proj_" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_folder);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_folder))
                Directory.Delete(_folder, true);
        }

        [TestMethod]
        public void CreateOrOpen_GravaOArquivoDeProjeto()
        {
            var project = GameProject.CreateOrOpen(_folder, "MeuJogo");

            Assert.IsTrue(File.Exists(project.ProjectFilePath));
            Assert.AreEqual("MeuJogo", project.Name);
        }

        [TestMethod]
        public void EnumerateScenes_ListaOsArquivosDbscene()
        {
            var project = GameProject.CreateOrOpen(_folder, "Jogo");
            SceneSerializer.Save(new Scene { Name = "Fase1" }, project.ScenePath("Fase1.dbscene"));
            SceneSerializer.Save(new Scene { Name = "Fase2" }, project.ScenePath("Fase2.dbscene"));

            var scenes = project.EnumerateScenes().ToList();

            CollectionAssert.AreEquivalent(new[] { "Fase1.dbscene", "Fase2.dbscene" }, scenes);
        }

        [TestMethod]
        public void Load_PreservaONome()
        {
            var created = GameProject.CreateOrOpen(_folder, "Projeto X");
            var loaded = GameProject.Load(created.ProjectFilePath);

            Assert.AreEqual("Projeto X", loaded.Name);
            Assert.AreEqual(_folder, loaded.Folder);
        }
    }
}
