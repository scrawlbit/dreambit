using System;

namespace DreamBit.Project.Exceptions
{
    public class TypeNotRegisteredException : Exception
    {
        public TypeNotRegisteredException(string type) : base($"The type {type} is not registred")
        {
        }
    }
}