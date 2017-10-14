using System;

namespace Minor.WSA.Infrastructure
{
    /// <summary>
    /// This attribute should decorate each event listening class.
    /// The QueueName is the name of the RabbitMQ-queue on which it will listen to incoming events.
    /// </summary>
    public class EventListenerAttribute : Attribute
    {
        public string QueueName { get; }

        public EventListenerAttribute(string queueName)
        {
            QueueName = queueName;
        }
    }
}