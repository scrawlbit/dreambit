using System;
using System.Linq;
using System.Linq.Expressions;
using Scrawlbit.Injection.Configuration;
using SimpleInjector;

namespace Scrawlbit.Injection
{
    public class ContainerBuilder
    {
        private readonly Container _container;
        private readonly RegistrationBuilder _builder;
        private readonly ResolverContainer _resolver;

        public ContainerBuilder()
        {
            _container = new Container();
            _resolver = new ResolverContainer(_container);
            _builder = new RegistrationBuilder(_container, _resolver);
        }

        public IRegistrationBuilder RegistrationBuilder => _builder;
        public bool AllowOverrides
        {
            get => _container.Options.AllowOverridingRegistrations;
            set => _container.Options.AllowOverridingRegistrations = value;
        }

        public IContainer Build()
        {
            _container.ResolveUnregisteredType += ResolveFuncFactories;
            _container.RegisterInstance<IContainer>(_resolver);

            _builder.Build();

            return _resolver;
        }

        private void ResolveFuncFactories(object sender, UnregisteredTypeEventArgs e)
        {
            var type = e.UnregisteredServiceType;

            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Func<>))
                return;

            var instanceType = type.GetGenericArguments().First();
            var registration = _container.GetRegistration(instanceType, true);
            
            Func<object> getInstance = () => registration.GetInstance();

            var funcType = typeof(Func<>).MakeGenericType(instanceType);
            
            var call = Expression.Call(Expression.Constant(getInstance.Target), getInstance.Method);
            var cast = Expression.Convert(call, instanceType);
            var lambda = Expression.Lambda(funcType, cast);
            
            e.Register(lambda);
        }
    }
}