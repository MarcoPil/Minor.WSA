using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Minor.WSA.Infrastructure
{
    public class TransientFactory : IFactory
    {
        private Type _type;
        private IServiceCollection _serviceCollection;

        public TransientFactory(IServiceCollection serviceCollection, Type type)
        {
            _type = type;
            _serviceCollection = serviceCollection;
        }

        public object GetInstance()
        {
            var serviceProvider = _serviceCollection.BuildServiceProvider();
            var result = ActivatorUtilities.CreateInstance(serviceProvider, _type);
            return result;
        }
    }
}