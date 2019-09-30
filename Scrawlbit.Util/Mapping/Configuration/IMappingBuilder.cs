namespace ScrawlBit.Mapping.Configuration
{
    public interface IMappingBuilder
    {
        IMappingSource<TSource> Map<TSource>();

        void RegisterProfile(IMappingProfile profile);
        void RegisterProfile<T>() where T : IMappingProfile, new();
    }
}