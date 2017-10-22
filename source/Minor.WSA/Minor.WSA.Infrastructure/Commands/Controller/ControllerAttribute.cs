using System;

namespace Minor.WSA.Infrastructure
{
    /// <summary>
    /// This attribute should decorate each event listening class.
    /// The QueueName is the name of the RabbitMQ-queue on which it will listen to incoming events.
    /// </summary>
    public class ControllerAttribute : Attribute
    {
        public string QueueName { get; }

        public ControllerAttribute(string queueName)
        {
            QueueName = queueName;
        }
    }
}