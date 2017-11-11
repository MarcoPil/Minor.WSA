using System;
using System.Collections.Generic;

namespace Minor.WSA.Infrastructure
{
    /// <summary>
    /// This core class for Microservices hosts eventhandlers (for handling events) and controllers (for handling commands).
    /// </summary>
    public class MicroserviceHost : IDisposable
    {
        private bool _isListening;
        public BusOptions BusOptions { get; }
        public IEnumerable<IEventListener> EventListeners { get; }
        public IEnumerable<IController> Controllers { get; }

        public MicroserviceHost(IEnumerable<IEventListener> eventListeners,
                                IEnumerable<IController> controllers,
                                BusOptions busOptions) 
        {
            BusOptions = busOptions ?? new BusOptions();
            EventListeners = eventListeners;
            Controllers = controllers;
            _isListening = false;
        }

        /// <summary>
        /// Declares an Exchange, starts listening en starts processing events.
        /// This method combines .StartListening() and .StartHandling().
        /// </summary>
        public void Start()
        {
            StartListening();
            StartHandling();
        }

        /// <summary>
        /// Declares an Exchange (as configured in the BusOptions). For each EventHander that is configured for this host, a queue (as configured in the EventHandlerAttribute) is opened and bound to this exchange. 
        /// From this moment in time, all relevant events are captured in the queue(s). 
        /// The event are only processed after the .StartHandling() method has been called.
        /// </summary>
        public void StartListening()
        {
            BusOptions.Provider.CreateConnection();

            foreach (var listener in EventListeners)
            {
                listener.OpenEventQueue(BusOptions);
            }
            foreach (var controller in Controllers)
            {
                controller.OpenCommandQueue(BusOptions);
            }
            _isListening = true;
        }

        /// <summary>
        /// Start processing the events that have arrived in the opened queues.
        /// </summary>
        public void StartHandling()
        {
            if (!_isListening)
            {
                throw new MicroserviceException("A MicroserviceHost can only start handling after start listening. Consider calling .StartListening() first.");
            }
            else
            {
                foreach (var listener in EventListeners)
                {
                    listener.StartHandling();
                }
                foreach (var controller in Controllers)
                {
                    controller.StartHandling();
                }
            }
        }

        public void Dispose()
        {
            BusOptions.Provider.Dispose();
        }
    }
}