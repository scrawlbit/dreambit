using System;

namespace DreamBit.Project.Exceptions
{
    public class FileIdAlreadyExistsException : Exception
    {
        public FileIdAlreadyExistsException() : base("There is alredy a file with the same id in the project") { }
    }
}