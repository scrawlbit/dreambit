using System;

namespace DreamBit.Pipeline.Exceptions
{
    public class ImportAlreadyExistsException : Exception
    {
        public ImportAlreadyExistsException(string path) : base($"There is already an import with the path \"{path}\"")
        {
        }
    }
}