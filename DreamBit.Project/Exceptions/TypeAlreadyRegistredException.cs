using System;

namespace DreamBit.Project.Exceptions
{
    public class TypeAlreadyRegistredException : Exception
    {
        public TypeAlreadyRegistredException() : base("This type is already registred")
        {
        }
    }
}