using System;
using System.Linq.Expressions;

namespace Scrawlbit.Mapping.Configuration
{
    public interface IMappingMemberConfiguration<TSource, TDestination, in TDestinationMember>
    {
        IMappingDestination<TSource, TDestination> Ignore();
        IMappingDestination<TSource, TDestination> Skip();
        IMappingDestination<TSource, TDestination> As<TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember);
        IMappingDestination<TSource, TDestination> Resolving<TResult>(Func<TSource, TResult> resolver);
        IMappingDestination<TSource, TDestination> Resolving(Func<TSource, TDestinationMember> resolver);
        IMappingDestination<TSource, TDestination> Resolving(Func<TDestinationMember> resolver);
    }
}