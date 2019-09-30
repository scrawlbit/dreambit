using System;

namespace ScrawlBit.Mapping.Configuration
{
    public interface IInclusionMappingSource<TSource, TDestination, TDerivedSource> where TDerivedSource : TSource
    {
        IMappingDestination<TSource, TDestination> To<TDerivedDestination>(Action<IMappingDestination<TDerivedSource, TDerivedDestination>> include = null) where TDerivedDestination : TDestination;
    }
}