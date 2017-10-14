using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minor.WSA.Infrastructure.Shared.TestBus
{
    public class TestBusProvider : IBusProvider
    {
        private Dictionary<string, EventQueue> _namedQueueus;
        public void CreateConnection()
        {
            _namedQueueus = new Dictionary<string, EventQueue>();
        }

        public void CreateQueueWithTopics(string queueName, IEnumerable<string> topicExpressions)
        {
            var eventQueue = new EventQueue(queueName, topicExpressions);
            _namedQueueus.Add(queueName, eventQueue);
        }
        public void PublishRawMessage(long timestamp, string routingKey, string correlationId, string eventType, string jsonMessage)
        {
            var eventMessage = new EventMessage(timestamp, routingKey, correlationId, eventType, jsonMessage);
        }

        public void StartReceiving(string queueName, EventReceivedCallback callback)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}
