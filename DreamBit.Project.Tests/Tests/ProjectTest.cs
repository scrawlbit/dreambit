using System;
using DreamBit.Project.Exceptions;
using DreamBit.Project.Mocks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DreamBit.Project.Tests
{
    // Atualizado para a API atual de Project/IProject. Foram removidos os testes
    // que exercitavam API que não existe mais: AddRegistration/Registrations
    // (migraram para IFileRegistrations via DI) e o Save incondicional
    // (hoje Save() só persiste quando há alterações pendentes).
    [TestClass]
    public class ProjectTest
    {
        private FileManagerMock _fileManager;
        private SerializerMock _serializer;
        private Project _project;

        [TestInitialize]
        public void Initialize()
        {
            _fileManager = new FileManagerMock();
            _serializer = new SerializerMock();
            _project = new Project(_serializer, _fileManager, new FileRegistrationsMock());
        }

        [TestMethod]
        public void Instantiation()
        {
            _project.Loaded.Should().BeFalse();
        }

        [TestMethod]
        public void Load()
        {
            _fileManager.ExistentFile = @"D:\Projects\Test\Test.dream";
            _project.Load(@"D:\Projects\Test\Test.dream");

            _project.Path.Should().Be(@"D:\Projects\Test\Test.dream");
            _project.Loaded.Should().BeTrue();
            _serializer.Project.Should().Be(_project);
        }

        [TestMethod]
        [ExpectedException(typeof(ProjectFileNotFoundException))]
        public void LoadInexistentFile()
        {
            _project.Load(@"D:\Projects\Test\Test.dream");
        }

        [TestMethod]
        [ExpectedException(typeof(ProjectAlreadyLoadedException))]
        public void LoadAlreadyDone()
        {
            _fileManager.ExistentFile = @"D:\Projects\Test\Test.dream";

            _project.Load(@"D:\Projects\Test\Test.dream");
            _project.Load(@"D:\Projects\Test\Test.dream");
        }

        [TestMethod]
        public void IncludeFilesWithoutOrder()
        {
            LoadProject();

            var file1 = ProjectFileMock.Script(_project, Guid.NewGuid(), @"Bosses\Boss2.cs");
            var file2 = ProjectFileMock.Script(_project, Guid.NewGuid(), @"Bosses\Boss1.cs");

            _project.IncludeFile(file1);
            _project.IncludeFile(file2);

            _project.Files.Should().BeEquivalentTo(new[] { file2, file1 });
        }

        [TestMethod]
        [ExpectedException(typeof(FileLocationAlreadyExistsException))]
        public void AddExistentFileLocation()
        {
            LoadProject();

            var file1 = ProjectFileMock.Script(_project, Guid.NewGuid(), @"Bosses\Boss.cs");
            var file2 = ProjectFileMock.Script(_project, Guid.NewGuid(), @"Bosses\Boss.cs");

            _project.IncludeFile(file1);
            _project.IncludeFile(file2);
        }

        private void LoadProject()
        {
            _fileManager.ExistentFile = @"D:\Projects\Test\Test.dream";
            _project.Load(@"D:\Projects\Test\Test.dream");
        }
    }
}
