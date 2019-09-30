using System;
using System.Linq.Expressions;
using AutoMapper;

namespace ScrawlBit.Mapping.Configuration
{
    internal class MappingMemberConfiguration<TSource, TDestination, TDestinationMember> : IMappingMemberConfiguration<TSource, TDestination, TDestinationMember>
    {
        private readonly MappingDestination<TSource, TDestination> _mapping;
        private readonly IMappingExpression<TSource, TDestination> _mappingExpression;
        private readonly Expression<Func<TDestination, TDestinationMember>> _destinationMember;

        internal MappingMemberConfiguration(
            MappingDestination<TSource, TDestination> mapping,
            IMappingExpression<TSource, TDestination> mappingExpression,
            Expression<Func<TDestination, TDestinationMember>> destinationMember)
        {
            _mapping = mapping;
            _mappingExpression = mappingExpression;
            _destinationMember = destinationMember;
        }

        public IMappingDestination<TSource, TDestination> Ignore()
        {
            return Options(opt => opt.Ignore());
        }
        public IMappingDestination<TSource, TDestination> Skip()
        {
            return Options(opt =>
            {
                opt.Ignore();
                opt.UseDestinationValue();
            });
        }
        public IMappingDestination<TSource, TDestination> As<TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember)
        {
            return Options(opt => opt.MapFrom(sourceMember));
        }
        public IMappingDestination<TSource, TDestination> Resolving<TResult>(Func<TSource, TResult> resolver)
        {
            return Options(opt => opt.MapFrom((s, d) => resolver(s)));
        }
        public IMappingDestination<TSource, TDestination> Resolving(Func<TSource, TDestinationMember> resolver)
        {
            return Options(opt => opt.MapFrom((s, d) => resolver(s)));
        }
        public IMappingDestination<TSource, TDestination> Resolving(Func<TDestinationMember> resolver)
        {
            return Resolving(src => resolver());
        }

        private MappingDestination<TSource, TDestination> Options(Action<IMemberConfigurationExpression<TSource, TDestination, TDestinationMember>> options)
        {
            _mappingExpression.ForMember(_destinationMember, options);
            return _mapping;
        }
    }
}