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

        public void Open()
        {
            base.CreateConnection();

            foreach (var listener in EventListeners)
            {
                listener.OpenEventQueue(Channel, BusOptions.ExchangeName);
            }
        }

        public void Start()
        {
            foreach (var listener in EventListeners)
            {
                listener.StartProcessing();
            }
        }
    }
}