using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minor.WSA.Infrastructure.Shared.TestBus
{
    public class TestBusProvider : IBusProvider
    {
        private Dictionary<string, TestEventQueue> _namedQueueus;
        public TestBusProvider()
        {
            _namedQueueus = new Dictionary<string, TestEventQueue>();
        }
        public void CreateConnection()
        {
        }

        public void CreateQueueWithTopics(string queueName, IEnumerable<string> topicExpressions)
        {
            var eventQueue = new TestEventQueue(queueName, topicExpressions);
            _namedQueueus.Add(queueName, eventQueue);
        }
        public void PublishRawMessage(long timestamp, string routingKey, string correlationId, string eventType, string jsonMessage)
        {
            var eventMessage = new EventMessage(timestamp, routingKey, correlationId, eventType, jsonMessage);
            foreach (var eventQueue in _namedQueueus.Values)
            {
                eventQueue.EnqueueIfMatches(routingKey, eventMessage);
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
