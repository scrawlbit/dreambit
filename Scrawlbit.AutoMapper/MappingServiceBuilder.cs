using AutoMapper;
using Scrawlbit.Mapping.Configuration;

namespace Scrawlbit.Mapping
{
    public class MappingServiceBuilder
    {
        private readonly MappingServiceProfile _profile;
        private readonly MappingService _mappingService;

        public MappingServiceBuilder()
        {
            _mappingService = new MappingService();
            _profile = new MappingServiceProfile();

            MappingBuilder = new MappingBuilder(_profile, _mappingService);
        }

        public IMappingBuilder MappingBuilder { get; }

        public IMappingService Build()
        {
            var configuration = new MapperConfiguration(c => c.AddProfile(_profile));
            var mapper = configuration.CreateMapper();
            
            _mappingService.Mapper = mapper;

            return _mappingService;
        }
    }
}