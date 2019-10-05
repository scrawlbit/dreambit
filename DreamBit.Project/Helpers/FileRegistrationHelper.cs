using DreamBit.Project.Registrations;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DreamBit.Project.Helpers
{
    internal static class FileRegistrationHelper
    {
        public static IFileRegistration DetermineFromPath(this IEnumerable<IFileRegistration> registrations, string path)
        {
            string extension = Path.GetExtension(path);
            IFileRegistration registration = registrations.Single(r => r.Extension == extension);

            return registration;
        }

        public static IFileRegistration DetermineFromType(this IEnumerable<IFileRegistration> registrations, string type)
        {
            return registrations.Single(r => r.Type == type);
        }
    }
}
