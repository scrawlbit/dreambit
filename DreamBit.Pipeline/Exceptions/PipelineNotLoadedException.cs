using System;

namespace DreamBit.Pipeline.Exceptions
{
    public class PipelineNotLoadedException : Exception
    {
        public PipelineNotLoadedException() : base("The pipeline is not loaded")
        {
        }
    }
}