namespace ScrawlBit.Mapping.Configuration
{
    public interface IMappingSource<TSource>
    {
        IMappingDestination<TSource, TSource> ToSelf();
        IMappingDestination<TSource, TDestination> To<TDestination>();
    }
}