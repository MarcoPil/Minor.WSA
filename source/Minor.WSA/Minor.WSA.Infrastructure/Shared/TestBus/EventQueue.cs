using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Minor.WSA.Infrastructure.Shared.TestBus
{
    internal class EventQueue
    {
        public string QueueName { get; }
        public List<string> Topics { get; }
        public ConcurrentQueue<EventMessage> EventMessages { get; }

        public EventQueue(string queueName, IEnumerable<string> topicExpressions)
        {
            QueueName = queueName;
            Topics = topicExpressions.ToList();
            EventMessages = new ConcurrentQueue<EventMessage>();
        }
    }
}