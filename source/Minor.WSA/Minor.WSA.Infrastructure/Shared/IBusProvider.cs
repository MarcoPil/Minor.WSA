using System;
using System.Collections.Generic;

namespace Minor.WSA.Infrastructure
{
    public interface IBusProvider
    {
        void CreateConnection();
        void PublishRawMessage(long timestamp, string routingKey, string correlationId, string eventType, string jsonMessage);
        void CreateQueueWithTopics(string queueName, IEnumerable<string> topicExpressions);
        void StartReceiving(string queueName, EventReceivedCallback callback);
        void Dispose();
    }

    public delegate void EventReceivedCallback(EventReceivedArgs args);
    public class EventReceivedArgs : EventArgs
    {
        public string RoutingKey { get; set; }
        public string Json { get; set; }
    }

}