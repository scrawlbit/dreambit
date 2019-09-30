using System;

namespace DreamBit.Pipeline.Exceptions
{
    public class PipelineAlreadyLoadedException : Exception
    {
        public PipelineAlreadyLoadedException() : base("The pipeline is already loaded")
        {
        }
    }
}