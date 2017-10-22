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
    /// For each event listening class (marked with the [EventListener(queueName)]-attribute) and EventListener is created.
    /// This Eventlistener is responsible for receiving all events that arrive at this particular queue. Therefore 
    /// there can be no two EventListeners that listen to the same queue
    /// </summary>
    public class EventListener : IEventListener
    {
        private Dictionary<string, IEventDispatcher> _dispatchers; //    string = topic-expression
        private BusOptions _busOptions;

        public string QueueName { get; }
        public IEnumerable<string> TopicExpressions => _dispatchers.Keys;

        public EventListener(string queueName, Dictionary<string, IEventDispatcher> dispatchers)
        {
            QueueName = queueName;
            _dispatchers = dispatchers;
        }

        /// <summary>
        /// Open a named queue (QueueName), so that the same queue can be reused when an off-line application come back on-line,
        /// and bind it to the exchange over ALL registered topic expressions (TopicExpressions).
        /// </summary>
        /// <param name="busOptions">The busOptions, which includes the BusProvider</param>
        public virtual void OpenEventQueue(BusOptions busOptions)
        {
            _busOptions = busOptions;

            busOptions.Provider.CreateQueueWithTopics(QueueName, TopicExpressions);

            // (from this moment in time, all relevant events are captured in the queue, for later processing)
        }

        /// <summary>
        /// Start handling events, i.e. start popping events from the queue and process them
        /// </summary>
        public virtual void StartHandling()
        {
            _busOptions.Provider.StartReceivingEvents(QueueName, EventReceived);
        }

        protected virtual void EventReceived(EventMessage eventMessage)
        {
            var matchingKeys = TopicExpressions.ThatMatch(eventMessage.RoutingKey);

            foreach (string matchingKey in matchingKeys)
            {
                var dispatcher = _dispatchers[matchingKey];
                dispatcher.DispatchEvent(eventMessage);
            }
        }
    }
}
