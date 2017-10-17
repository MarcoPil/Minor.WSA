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
        private List<EventListener> _eventListeners;
        private BusOptions _busOptions;

        public IEnumerable<string> Factories => _factories.Keys.Select(t => t.ToString());
        public IEnumerable<EventListener> EventListeners => _eventListeners;
        public IServiceCollection ServiceProvider => _serviceCollection;

        public MicroserviceHostBuilder()
        {
            _serviceCollection = new ServiceCollection();
            _factories = new Dictionary<Type, IFactory>();
            _eventListeners = new List<EventListener>();
            _busOptions = default(BusOptions);
        }

        public MicroserviceHostBuilder WithBusOptions(BusOptions options)
        {
            _busOptions = options;
            return this;        // for call chaining
        }

        public MicroserviceHostBuilder UseConventions()
        {
            FindEventListeners();
            return this;        // for call chaining
        }

        public MicroserviceHostBuilder AddEventListener<T>()
        {
            FindEventListeners(typeof(T));
            return this;        // for call chaining
        }

        private void FindEventListeners()
        {
            var thisAssembly = this.GetType().GetTypeInfo().Assembly;
            var referencingAssemblies = GetRefencingAssemblies(thisAssembly);

            foreach (var type in referencingAssemblies.SelectMany(a => a.GetTypes()))
            {
                FindEventListeners(type);
            }
        }

        private void FindEventListeners(Type type)
        {
            var eventListenerAttr = type.GetTypeInfo().GetCustomAttribute<EventListenerAttribute>();
            if (eventListenerAttr != null)
            {
                var factory = new TransientFactory(_serviceCollection, type);
                _factories.Add(type, factory);
                _eventListeners.Add(CreateEventListener(type, eventListenerAttr.QueueName, factory));
            }
        }

        private IEnumerable<Assembly> GetRefencingAssemblies(Assembly assembly)
        {
            var assemblyName = assembly.FullName;
            return from library in DependencyContext.Default.RuntimeLibraries
                   where assemblyName.StartsWith(library.Name)
                         || library.Dependencies.Any(d => assemblyName.StartsWith(d.Name))
                   select Assembly.Load(new AssemblyName(library.Name));
        }

        private EventListener CreateEventListener(Type type, string queueName, IFactory factory)
        {
            var eventHandlers = new Dictionary<string, EventDispatcher>();

            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            foreach (var method in methods)
            {
                var (topicExpression, dispatcher) = CreateEventHandler(type, factory, method);
                if (topicExpression != null)
                {
                    if (eventHandlers.ContainsKey(topicExpression))
                    {
                        throw new MicroserviceConfigurationException($"Two topic expressions cannot be exactly identical. The topic expression '{topicExpression}' has already been registered.");
                    }
                    else
                    {
                        eventHandlers.Add(topicExpression, dispatcher);
                    }
                }

            }
            return new EventListener(queueName, eventHandlers);
        }

        private static (string, EventDispatcher) CreateEventHandler(Type type, IFactory factory, MethodInfo method)
        {
            if (method.GetParameters().Length == 1)
            {
                string topicExpression = null;

                var paramType = method.GetParameters().First().ParameterType;
                if (typeof(DomainEvent).IsAssignableFrom(paramType))
                {
                    topicExpression = TopicFromAttributeOrDefault(method);

                    if (topicExpression == null)
                    {
                        string typeName = type.Name;
                        if (typeName.EndsWith("EventListener"))
                        {
                            int pos = typeName.LastIndexOf("EventListener");
                            typeName = typeName.Substring(0, pos);
                        }
                        topicExpression = "#." + typeName + "." + paramType.Name;
                    }

                    var dispatcher = new EventDispatcher(factory, method, paramType);
                    return (topicExpression, dispatcher);
                }
                else if (paramType == typeof(EventMessage))
                {
                    topicExpression = TopicFromAttributeOrDefault(method) ?? "#";

                    var dispatcher = new EventDispatcher(factory, method, paramType);
                    return (topicExpression, dispatcher);
                }
            }
            return (null, null);
        }

        private static string TopicFromAttributeOrDefault(MethodInfo method)
        {
            var topicAttr = method.GetCustomAttribute<TopicAttribute>();
            string topicExpression = topicAttr?.Topic;
            if (topicExpression == null && topicAttr != null  ||
                topicExpression != null && !RoutingKeyMatcher.IsValidTopicExpression(topicExpression))
            {
                throw new MicroserviceConfigurationException($"Topic Expression '{topicExpression}' has an invalid expression format.");
            }
            return topicExpression;
        }

        public MicroserviceHost CreateHost()
        {
            var host = new MicroserviceHost(EventListeners, _busOptions);

            return host;
        }

    }
}