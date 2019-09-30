using System;

namespace DreamBit.Project.Exceptions
{
    public class InvalidFileExtensionException : Exception
    {
        public InvalidFileExtensionException(string path, string expectedExtension) : base(
            $"The path {path} is invalid for this file. The expected extension was {expectedExtension}.")
        {
        }
    }
}