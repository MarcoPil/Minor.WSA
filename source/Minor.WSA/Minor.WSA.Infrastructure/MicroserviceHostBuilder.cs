using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Minor.WSA.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Minor.WSA.Infrastructure
{
    public class MicroserviceHostBuilder
    {
        private IServiceCollection _serviceCollection;
        private Dictionary<Type, IFactory> _factories;
        private Dictionary<string, EventDispatcher> _eventHandles;

        public IEnumerable<string> Factories => _factories.Keys.Select(t => t.ToString());
        public IEnumerable<string> EventHandles => _eventHandles.Keys;

        public MicroserviceHostBuilder()
        {
            _serviceCollection = new ServiceCollection();
            _factories = new Dictionary<Type, IFactory>();
            _eventHandles = new Dictionary<string, EventDispatcher>();
        }

        public MicroserviceHostBuilder UseConventions()
        {
            FindEventHandlers();
            return this;
        }

        private void FindEventHandlers()
        {
            var thisAssembly = this.GetType().GetTypeInfo().Assembly;
            var referencingAssemblies = GetRefencingAssemblies(thisAssembly);

            foreach (var type in referencingAssemblies.SelectMany(a => a.GetTypes()))
            {
                FindEventHandlers(type);
            }
        }

        private void FindEventHandlers(Type type)
        {
            if (type.GetTypeInfo().GetCustomAttributes<EventHandlerAttribute>().Any())
            {
                _factories.Add(type, new TransientFactory(_serviceCollection, type));
                CreateEventDispatchers(type);
            }
        }

        public MicroserviceHostBuilder AddEventHandler<T>()
        {
            FindEventHandlers(typeof(T));
            return this;
        }

        private IEnumerable<Assembly> GetRefencingAssemblies(Assembly assembly)
        {
            var assemblyName = assembly.FullName;
            return from library in DependencyContext.Default.RuntimeLibraries
                   where assemblyName.StartsWith(library.Name)
                         || library.Dependencies.Any(d => assemblyName.StartsWith(d.Name))
                   select Assembly.Load(new AssemblyName(library.Name));
        }

        private void CreateEventDispatchers(Type type)
        {
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            foreach (var method in methods)
            {
                if (method.GetParameters().Length == 1)
                {
                    var paramType = method.GetParameters().First().ParameterType;
                    if (typeof(DomainEvent).IsAssignableFrom(paramType))
                    {
                        string routingKey;

                        var routingKeyAttr = method.GetCustomAttribute<RoutingKeyAttribute>();
                        if (routingKeyAttr != null)
                        {
                            routingKey = routingKeyAttr.RoutingKey;
                        }
                        else
                        {
                            string typeName = type.Name;
                            if (typeName.EndsWith("EventHandler"))
                            {
                                int pos = typeName.LastIndexOf("EventHandler");
                                typeName = typeName.Substring(0, pos);
                            }
                            routingKey = "#." + typeName + "." + paramType.Name;
                        }
                        var factory = _factories[type];
                        _eventHandles.Add(routingKey, new EventDispatcher(factory,method,paramType));
                    }
                    //if (paramType == typeof(Newtonsoft.Json.Linq.JObject))
                    //{
                    //    _eventHandles.Add("#", new EventDispatcher(??, method, typeof(object)));
                    //}
                }
            }
        }

        public MicroserviceHost CreateHost()
        {
            var host = new MicroserviceHost();

            return host;
        }

    }
}