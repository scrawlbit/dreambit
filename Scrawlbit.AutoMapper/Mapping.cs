using System;
using AutoMapper;

namespace ScrawlBit.Mapping
{
    internal class Mapping<T> : IMapping
    {
        private readonly IMapper _mapper;
        private readonly T _model;

        public Mapping(IMapper mapper, T model)
        {
            _mapper = mapper;
            _model = model;
        }

        public object To(Type destinationType)
        {
            return _mapper.Map(_model, typeof(T), destinationType);
        }
        public TDestination To<TDestination>()
        {
            return _mapper.Map<T, TDestination>(_model);
        }
        public TDestination To<TDestination>(TDestination destination)
        {
            return _mapper.Map(_model, destination);
        }
    }
}