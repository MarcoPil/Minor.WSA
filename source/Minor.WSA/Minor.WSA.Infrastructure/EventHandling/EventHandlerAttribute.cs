using System;

namespace Minor.WSA.Infrastructure
{
    /// <summary>
    /// This attribute should decorate each eventhandling class.
    /// The QueueName is the name of the RabbitMQ-queue on which it will listen to incoming events.
    /// </summary>
    public class EventHandlerAttribute : Attribute
    {
        public string QueueName { get; }

        public EventHandlerAttribute(string queueName)
        {
            QueueName = queueName;
        }
    }
}