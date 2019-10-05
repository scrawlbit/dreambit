using DreamBit.Project.Registrations;
using Scrawlbit.Injection.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DreamBit.Project.Helpers
{
    public static class FileRegistrationHelper
    {
        public static IRegistration<T> RegisterFile<T>(this IRegistrationBuilder builder) where T : class, IFileRegistration
        {
            FileRegistrations.Types.Add(typeof(T));

            return builder.Register<T>();
        }
    }
}
