using System;
using DreamBit.Project.Registrations;

namespace DreamBit.Project.Mocks
{
    public class RegistrationMock : IFileRegistration
    {
        public string Type { get; set; }
        public string Extension { get; set; }

        public ProjectFile CreateInstance()
        {
            throw new NotImplementedException();
        }
    }
}