using System;

namespace DreamBit.Project.Exceptions
{
    public class FileLocationAlreadyExistsException : Exception
    {
        public FileLocationAlreadyExistsException() : base("There is alredy a file with the same location in the project") { }
    }
}