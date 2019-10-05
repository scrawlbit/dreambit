using DreamBit.Project.Exceptions;
using Scrawlbit.Injection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DreamBit.Project.Registrations
{
    internal interface IFileRegistrations
    {
        IFileRegistration DetermineFromPath(string path);
        IFileRegistration DetermineFromType(string type);
    }

    internal class FileRegistrations : IFileRegistrations
    {
        public static readonly List<Type> Types;
        private readonly IContainer _container;
        private List<IFileRegistration> _registrations;

        static FileRegistrations()
        {
            Types = new List<Type>();
        }
        public FileRegistrations(IContainer container)
        {
            _container = container;
        }

        private List<IFileRegistration> Registrations
        {
            get
            {
                if (_registrations == null)
                {
                    _registrations = new List<IFileRegistration>();

                    foreach (var type in Types)
                    {
                        var registration = _container.Resolve<IFileRegistration>(type);

                        if (_registrations.Contains(registration))
                            continue;

                        if (_registrations.Any(r => r.Type == registration.Type))
                            throw new TypeAlreadyRegistredException();

                        _registrations.Add(registration);
                    }
                }

                return _registrations;
            }
        }

        public IFileRegistration DetermineFromPath(string path)
        {
            string extension = Path.GetExtension(path);
            IFileRegistration registration = Registrations.Single(r => r.Extension == extension);

            return registration;
        }
        public IFileRegistration DetermineFromType(string type)
        {
            return Registrations.Single(r => r.Type == type);
        }
    }
}
