using System;

namespace Scrawlbit.Mapping
{
    public interface IMapping
    {
        object To(Type destinationType);
        TDestination To<TDestination>();
        TDestination To<TDestination>(TDestination destination);
    }
}