using System;
using DreamBit.Project.Registrations;

namespace DreamBit.Project.Mocks
{
    public class RegistrationMock : IProjectRegistration
    {
        public string Type { get; set; }
        public string Extension { get; set; }

        public ProjectFile CreateInstance()
        {
            throw new NotImplementedException();
        }
    }
}