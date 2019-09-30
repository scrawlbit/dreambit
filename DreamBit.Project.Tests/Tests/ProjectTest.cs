using System;
using DreamBit.Project.Exceptions;
using DreamBit.Project.Mocks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DreamBit.Project.Tests
{
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
            _project = new Project(_serializer, _fileManager);
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
        public void AddARegistration()
        {
            var registration = new RegistrationMock { Type = "Script" };

            _project.AddRegistration(registration);

            _project.Registrations.Should().Contain(registration);
        }

        [TestMethod]
        [ExpectedException(typeof(TypeAlreadyRegistredException))]
        public void AddAnExisitentTypeRegistration()
        {
            var registration1 = new RegistrationMock { Type = "Script" };
            var registration2 = new RegistrationMock { Type = "Script" };

            _project.AddRegistration(registration1);
            _project.AddRegistration(registration2);
        }

        [TestMethod]
        public void Save()
        {
            _fileManager.ExistentFile = @"D:\Projects\Test\Test.dream";
            _project.Load(@"D:\Projects\Test\Test.dream");
            _project.Save();

            _serializer.Saved.Should().BeTrue();
            _serializer.Project.Should().Be(_project);
        }

        [TestMethod]
        [ExpectedException(typeof(ProjectNotLoadedException))]
        public void SaveProjectNotLoaded()
        {
            _project.Save();
        }

        [TestMethod]
        public void IncludeFilesWithoutOrder()
        {
            LoadProject();

            var file1 = ProjectFileMock.Script(_project, Guid.NewGuid(), @"Bosses\Boss2.cs");
            var file2 = ProjectFileMock.Script(_project, Guid.NewGuid(), @"Bosses\Boss1.cs");

            _project.IncludeFile(file1);
            _project.IncludeFile(file2);

            _project.Files.Should().BeEquivalentTo(file2, file1);
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