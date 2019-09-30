using AutoMapper;

namespace ScrawlBit.Mapping.Configuration
{
    internal class MappingSource<TSource> : IMappingSource<TSource>
    {
        private readonly Profile _profile;
        private readonly IMappingService _mappingService;

        internal MappingSource(Profile profile, IMappingService mappingService)
        {
            _profile = profile;
            _mappingService = mappingService;
        }

        public IMappingDestination<TSource, TDestination> To<TDestination>()
        {
            return new MappingDestination<TSource, TDestination>(_profile, _mappingService);
        }
        public IMappingDestination<TSource, TSource> ToSelf()
        {
            return To<TSource>();
        }
    }
}