using System;

namespace DreamBit.Pipeline.Exceptions
{
    public class ImportNotFoundException : Exception
    {
        public ImportNotFoundException(string path) : base($"There is no import with the path \"{path}\"")
        {
        }
    }
}