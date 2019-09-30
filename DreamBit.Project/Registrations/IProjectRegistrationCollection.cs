using System.Collections.Generic;

namespace DreamBit.Project.Registrations
{
    public interface IProjectRegistrationCollection
    {
        IEnumerable<IProjectRegistration> Registrations();
    }
}