using System.Collections.Generic;
using DreamBit.Project.Registrations;

namespace DreamBit.Pipeline.Registrations
{
    public interface IPipelineRegistrations : IProjectRegistrationCollection
    {
        
    }

    internal class PipelineRegistrations : IPipelineRegistrations
    {
        private readonly IPipelineFontRegistration _fontRegistration;
        private readonly IPipelineImageRegistration _imageRegistration;

        public PipelineRegistrations(IPipelineFontRegistration fontRegistration, IPipelineImageRegistration imageRegistration)
        {
            _fontRegistration = fontRegistration;
            _imageRegistration = imageRegistration;
        }

        public IEnumerable<IFileRegistration> Registrations()
        {
            yield return _fontRegistration;
            yield return _imageRegistration;
        }
    }
}