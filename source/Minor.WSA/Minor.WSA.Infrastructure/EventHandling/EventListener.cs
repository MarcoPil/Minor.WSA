using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minor.WSA.Infrastructure
{
    /// <summary>
    /// For each EventHandler class (marked with the [EventHandler(queueName)]-attribute) and EventListener is created.
    /// This Eventlistener is responsible for receiving all events that arrive at this particular queue. Therefore 
    /// there can be no two EventListeners that listen to the same queue
    /// </summary>
    public class EventListener : IEventListener
    {
        private Dictionary<string, EventDispatcher> _dispatchers; //    string = routingkey-expression
        private BusOptions _busOptions;

        public string QueueName { get; }
        public IEnumerable<string> TopicExpressions => _dispatchers.Keys;

        public EventListener(string queueName, Dictionary<string, EventDispatcher> dispatchers)
        {
            QueueName = queueName;
            _dispatchers = dispatchers;
        }

        /// <summary>
        /// Open a named queue (QueueName), so that the same queue can be reused when an off-line application come back on-line,
        /// and bind it to the exchange over ALL registered routing key expressions (RoutingKeyExpressions).
        /// </summary>
        /// <param name="channel">An opened channel that represents a connection to an rabbitMQ service</param>
        /// <param name="exchangeName">The name of the topic-exchange to which the queue (QueueName) is bound - possibly multiple times, each time with a different routing key expression.</param>
        public virtual void OpenEventQueue(BusOptions busOptions)
        {
            _busOptions = busOptions;

            busOptions.Provider.CreateQueueWithTopics(QueueName, TopicExpressions);

            // (from this moment in time, all relevant events are captured in the queue, for later processing)
        }

        /// <summary>
        /// Start listening to events
        /// </summary>
        public virtual void StartProcessing()
        {
            _busOptions.Provider.StartReceiving(QueueName, EventReceived);
        }

        protected virtual void EventReceived(EventReceivedArgs args)
        {
            var matchingKeys = RoutingKeyMatcher.Match(args.RoutingKey, TopicExpressions);

            foreach (string matchingKey in matchingKeys)
            {
                var dispatcher = _dispatchers[matchingKey];
                dispatcher.DispatchEvent(args.Json);
            }
        }
    }
}
