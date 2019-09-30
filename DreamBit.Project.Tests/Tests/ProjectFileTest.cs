using System;
using DreamBit.Project.Mocks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DreamBit.Project.Tests
{
    [TestClass]
    public class ProjectFileTest
    {
        private ProjectMock _project;

        [TestInitialize]
        public void Initialize()
        {
            _project = new ProjectMock { Folder = @"C:\Projects\Test" };
        }

        [TestMethod]
        public void GetName()
        {
            var file = ProjectFileMock.Script(_project, @"Bosses\Boss.cs");

            file.Name.Should().Be("Boss.cs");
        }

        [TestMethod]
        public void DeterminePathAndLocation()
        {
            var file = ProjectFileMock.Script(_project, @"Bosses\Boss.cs");

            file.Location.Should().Be(@"Bosses\Boss.cs");
            file.Path.Should().Be(@"C:\Projects\Test\Bosses\Boss.cs");
        }
    }
}