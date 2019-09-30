using AutoMapper;

namespace ScrawlBit.Mapping.Configuration
{
    internal class MappingBuilder : IMappingBuilder
    {
        private readonly Profile _profile;
        private readonly IMappingService _mappingService;

        public MappingBuilder(Profile profile, IMappingService mappingService)
        {
            _profile = profile;
            _mappingService = mappingService;
        }

        public IMappingSource<TSource> Map<TSource>()
        {
            return new MappingSource<TSource>(_profile, _mappingService);
        }

        public void RegisterProfile(IMappingProfile profile)
        {
            profile.Register(this);
        }
        public void RegisterProfile<T>() where T : IMappingProfile, new()
        {
            RegisterProfile(new T());
        }
    }
}