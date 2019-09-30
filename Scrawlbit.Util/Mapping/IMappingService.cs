namespace ScrawlBit.Mapping
{
    public interface IMappingService
    {
        IMapping Map<T>(T model);
    }
}