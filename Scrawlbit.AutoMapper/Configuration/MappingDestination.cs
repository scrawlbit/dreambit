using System;
using System.Linq.Expressions;
using AutoMapper;

namespace Scrawlbit.Mapping.Configuration
{
    internal class MappingDestination<TSource, TDestination> : IMappingDestination<TSource, TDestination>
    {
        private readonly Profile _profile;
        private readonly IMappingService _mappingService;
        private readonly IMappingExpression<TSource, TDestination> _mappingExpression;

        internal MappingDestination(Profile profile, IMappingService mappingService, IMappingExpression<TSource, TDestination> mappingExpression = null)
        {
            _profile = profile;
            _mappingService = mappingService;
            _mappingExpression = mappingExpression ?? profile.CreateMap<TSource, TDestination>();

            MaxDepth(5);
        }

        public IMappingDestination<TSource, TDestination> MaxDepth(int depth)
        {
            _mappingExpression.MaxDepth(depth);
            return this;
        }
        public IMappingDestination<TSource, TDestination> AsMapping<T>() where T : TDestination
        {
            var service = _mappingService;

            _mappingExpression.ConstructUsing(src => service.Map(src).To<T>());
            return this;
        }
        public IMappingDestination<TSource, TDestination> Constructing(Func<TSource, TDestination> constructor)
        {
            _mappingExpression.ConstructUsing((s, c) => constructor(s));
            return this;
        }
        public IMappingDestination<TSource, TDestination> AfterMap(Action<TSource, TDestination> afterFunction)
        {
            _mappingExpression.AfterMap(afterFunction);
            return this;
        }
        public IMappingDestination<TSource, TDestination> BeforeMap(Action<TSource, TDestination> beforeFunction)
        {
            _mappingExpression.BeforeMap(beforeFunction);
            return this;
        }
        public IMappingMemberConfiguration<TSource, TDestination, TMember> Member<TMember>(Expression<Func<TDestination, TMember>> destinationMember)
        {
            return new MappingMemberConfiguration<TSource, TDestination, TMember>(this, _mappingExpression, destinationMember);
        }

        public IInclusionMappingSource<TSource, TDestination, TDerivedSource> Include<TDerivedSource>() where TDerivedSource : TSource
        {
            return new InclusionMappingSource<TSource, TDestination, TDerivedSource>(this, _mappingExpression, _profile, _mappingService);
        }
        public IMappingDestination<TSource, TDestination> IncludeSource<TDerivedSource>(Action<IMappingDestination<TDerivedSource, TDestination>> include) where TDerivedSource : TSource
        {
            return Include<TDerivedSource>().To(include);
        }
        public IMappingDestination<TSource, TDestination> IncludeDestination<TDerivedDestination>(Action<IMappingDestination<TSource, TDerivedDestination>> include) where TDerivedDestination : TDestination
        {
            return Include<TSource>().To(include);
        }
    }
}