using System;

namespace DreamBit.Game.Exceptions
{
    public class ContentNotRegisteredException : Exception
    {
        public ContentNotRegisteredException(Guid id) : base($"There is no content registered with id \"{id}\"")
        {
            
        }
    }
}