using System;
using System.Linq.Expressions;

namespace ScrawlBit.Mapping.Configuration
{
    public interface IMappingDestination<TSource, TDestination>
    {
        IMappingDestination<TSource, TDestination> MaxDepth(int depth);
        IMappingDestination<TSource, TDestination> AsMapping<T>() where T : TDestination;
        IMappingDestination<TSource, TDestination> Constructing(Func<TSource, TDestination> constructor);
        IMappingDestination<TSource, TDestination> AfterMap(Action<TSource, TDestination> afterFunction);
        IMappingDestination<TSource, TDestination> BeforeMap(Action<TSource, TDestination> beforeFunction);
        IMappingMemberConfiguration<TSource, TDestination, TMember> Member<TMember>(Expression<Func<TDestination, TMember>> destinationMember);
        
        IInclusionMappingSource<TSource, TDestination, TDerivedSource> Include<TDerivedSource>() where TDerivedSource : TSource;
        IMappingDestination<TSource, TDestination> IncludeSource<TDerivedSource>(Action<IMappingDestination<TDerivedSource, TDestination>> include = null) where TDerivedSource : TSource;
        IMappingDestination<TSource, TDestination> IncludeDestination<TDerivedDestination>(Action<IMappingDestination<TSource, TDerivedDestination>> include = null) where TDerivedDestination : TDestination;
    }
}