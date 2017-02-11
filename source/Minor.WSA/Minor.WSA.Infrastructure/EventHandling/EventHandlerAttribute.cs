using System;

namespace Minor.WSA.Infrastructure
{

    public class EventHandlerAttribute : Attribute
    {
        public string QueueName { get; }

        public EventHandlerAttribute(string queueName)
        {
            QueueName = queueName;
        }
    }
}