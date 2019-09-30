using System;

namespace ScrawlBit.Mapping
{
    public interface IMapping
    {
        object To(Type destinationType);
        TDestination To<TDestination>();
        TDestination To<TDestination>(TDestination destination);
    }
}