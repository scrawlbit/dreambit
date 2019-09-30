using System;
using AutoMapper;

namespace Scrawlbit.Mapping.Configuration
{
    internal class InclusionMappingSource<TSource, TDestination, TDerivedSource> : IInclusionMappingSource<TSource, TDestination, TDerivedSource>
        where TDerivedSource : TSource
    {
        private readonly IMappingDestination<TSource, TDestination> _baseMapping;
        private readonly IMappingExpression<TSource, TDestination> _baseExpression;
        private readonly IMappingSource<TDerivedSource> _mappingSource;

        internal InclusionMappingSource(
            IMappingDestination<TSource, TDestination> baseMapping,
            IMappingExpression<TSource, TDestination> baseExpression,
            Profile profile, IMappingService mappingService)
        {
            _baseMapping = baseMapping;
            _baseExpression = baseExpression;
            _mappingSource = new MappingSource<TDerivedSource>(profile, mappingService);
        }

        public IMappingDestination<TSource, TDestination> To<TDerivedDestination>(Action<IMappingDestination<TDerivedSource, TDerivedDestination>> include) where TDerivedDestination : TDestination
        {
            _baseExpression.Include<TDerivedSource, TDerivedDestination>();
            var destination = _mappingSource.To<TDerivedDestination>();

            include?.Invoke(destination);

            return _baseMapping;
        }
    }
}