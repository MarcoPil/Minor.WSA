using System;
using System.Collections.Generic;

namespace Minor.WSA.Infrastructure
{
    /// <summary>
    /// This core class for Microservices hosts eventhandlers (for handling events) and controllers (for handling commands).
    /// </summary>
    public class MicroserviceHost : EventBusBase
    {
        public IEnumerable<IEventListener> EventListeners { get; }

        public MicroserviceHost(IEnumerable<IEventListener> eventListeners, BusOptions busOptions = default(BusOptions)) 
            : base(busOptions)
        {
            EventListeners = eventListeners;
        }

        /// <summary>
        /// Declares an Exchange (as configured in the BusOptions). For each EventHander that is configured for this host, a queue (as configured in the EventHandlerAttribute) is opened and bound to this exchange. 
        /// From this moment in time, all relevant events are captured in the queue(s). 
        /// The event are only processed after the .Start() method has been called.
        /// </summary>
        public void Open()
        {
            base.CreateConnection();

            foreach (var listener in EventListeners)
            {
                listener.OpenEventQueue(Channel, BusOptions.ExchangeName);
            }
        }

        /// <summary>
        /// Start processing the events that have arrived in the opened queues.
        /// </summary>
        public void Start()
        {
            foreach (var listener in EventListeners)
            {
                listener.StartProcessing();
            }
        }
    }
}