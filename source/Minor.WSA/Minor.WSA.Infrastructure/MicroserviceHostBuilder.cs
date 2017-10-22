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
        private List<Controller> _controllers;
        private BusOptions _busOptions;

        public IEnumerable<string> Factories => _factories.Keys.Select(t => t.ToString());
        public IEnumerable<EventListener> EventListeners => _eventListeners;
        public IEnumerable<Controller> Controllers => _controllers;
        public IServiceCollection ServiceProvider => _serviceCollection;


        public MicroserviceHostBuilder()
        {
            _serviceCollection = new ServiceCollection();
            _factories = new Dictionary<Type, IFactory>();
            _eventListeners = new List<EventListener>();
            _controllers = new List<Controller>();
            _busOptions = default(BusOptions);
        }

        public MicroserviceHostBuilder WithBusOptions(BusOptions options)
        {
            _busOptions = options;
            return this;        // for call chaining
        }

        public MicroserviceHostBuilder UseConventions()
        {
            FindEventListenersAndControllers();
            return this;        // for call chaining
        }

        public MicroserviceHostBuilder AddEventListener<T>()
        {
            FindEventListeners(typeof(T));
            return this;        // for call chaining
        }

        public MicroserviceHostBuilder AddController<T>()
        {
            FindControllers(typeof(T));
            return this;        // for call chaining
        }

        public MicroserviceHost CreateHost()
        {
            var host = new MicroserviceHost(EventListeners, Controllers, _busOptions);

            return host;
        }

        private void FindEventListenersAndControllers()
        {
            var thisAssembly = this.GetType().GetTypeInfo().Assembly;
            var referencingAssemblies = GetRefencingAssemblies(thisAssembly);

            foreach (var type in referencingAssemblies.SelectMany(a => a.GetTypes()))
            {
                FindEventListeners(type);
                FindControllers(type);
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

        private void FindControllers(Type type)
        {
            var controllerAttr = type.GetTypeInfo().GetCustomAttribute<ControllerAttribute>();
            if (controllerAttr != null)
            {
                var factory = new TransientFactory(_serviceCollection, type);
                _factories.Add(type, factory);
                _controllers.Add(CreateController(type, controllerAttr.QueueName, factory));
            }
        }

        private EventListener CreateEventListener(Type type, string queueName, IFactory factory)
        {
            var eventHandlers = new Dictionary<string, IEventDispatcher>();

            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            foreach (var method in methods)
            {
                var (topicExpression, dispatcher) = CreateEventHandlerForMethod(type, factory, method);
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

        private Controller CreateController(Type type, string queueName, TransientFactory factory)
        {
            var commandHandlers = new Dictionary<string, IEventDispatcher>();

            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            foreach (var method in methods)
            {
                var (command, dispatcher) = CreateCommandHandlerForMethod(type, factory, method);
                if (command != null)
                {
                    if (commandHandlers.ContainsKey(command))
                    {
                        throw new MicroserviceConfigurationException($"Two commands cannot be exactly identical. The command '{command}' has already been registered.");
                    }
                    else
                    {
                        commandHandlers.Add(command, dispatcher);
                    }
                }

            }
            return new Controller(queueName, commandHandlers);
        }

        private static (string, IEventDispatcher) CreateCommandHandlerForMethod(Type type, IFactory factory, MethodInfo method)
        {
            var executeAttr = method.GetCustomAttribute<ExecuteAttribute>();

            if (method.GetParameters().Length == 1 &&
                (method.Name == "Execute" || executeAttr != null))
            {
                var paramType = method.GetParameters().Single().ParameterType;
                var commandType = executeAttr?.CommandTypeName  ??  paramType.ToString();
                var dispatcher = new EventDispatcher(factory, method, paramType);
                return (commandType, dispatcher);
            }
            return (null, null);
        }

        private static (string, IEventDispatcher) CreateEventHandlerForMethod(Type type, IFactory factory, MethodInfo method)
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

                    var dispatcher = new GenericEventDispatcher(factory, method);
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


    }
}