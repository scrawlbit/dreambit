using DreamBit.Pipeline.Imports;
using Scrawlbit.Mapping.Configuration;

namespace DreamBit.Pipeline.Properties
{
    public class PipelineMappingProfile : IMappingProfile
    {
        public void Register(IMappingBuilder builder)
        {
            builder.Map<FontImport>().ToSelf();
            builder.Map<TextureImport>().ToSelf();
        }
    }
}