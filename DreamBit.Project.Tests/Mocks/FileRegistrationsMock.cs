using System;
using DreamBit.Project.Registrations;

namespace DreamBit.Project.Mocks
{
    /// <summary>
    /// IFileRegistrations para satisfazer o construtor de Project nos testes.
    /// As determinações não são exercitadas pelos testes atuais (Load/IncludeFile).
    /// </summary>
    internal class FileRegistrationsMock : IFileRegistrations
    {
        public IFileRegistration DetermineFromPath(string path) => throw new NotImplementedException();
        public IFileRegistration DetermineFromType(string type) => throw new NotImplementedException();
        public IFileRegistration DetermineFromObjectType(Type objectType) => throw new NotImplementedException();
    }
}
