using AutoMapper;

namespace ScrawlBit.Mapping
{
    internal class MappingService : IMappingService
    {
        public IMapper Mapper { get; set; }
        
        public IMapping Map<T>(T model)
        {
            return new Mapping<T>(Mapper, model);
        }
    }
}