using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minor.WSA.Infrastructure.Shared.TestBus
{
    public class TestBusProvider : IBusProvider
    {
        private Dictionary<string, TestEventQueue> _namedQueueus;

        public List<EventMessage> LoggedMessages { get; }

        public TestBusProvider()
        {
            _namedQueueus = new Dictionary<string, TestEventQueue>();
            LoggedMessages = new List<EventMessage>();
        }
        public void CreateConnection()
        {
        }

        public void CreateQueueWithTopics(string queueName, IEnumerable<string> topicExpressions)
        {
            var eventQueue = new TestEventQueue(queueName, topicExpressions);
            _namedQueueus.Add(queueName, eventQueue);
        }
        public void PublishEventMessage(EventMessage eventMessage)
        {
            LoggedMessages.Add(eventMessage);
            foreach (var eventQueue in _namedQueueus.Values)
            {
                eventQueue.EnqueueIfMatches(eventMessage.RoutingKey, eventMessage);
            }
        }

        public void StartReceiving(string queueName, EventReceivedCallback callback)
        {
            _namedQueueus[queueName].StartDequeueing(callback);
        }

        public void Dispose()
        {
        }
    }
}
