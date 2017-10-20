using System;
using System.Collections.Generic;

namespace Minor.WSA.Infrastructure
{
    public interface IBusProvider
    {
        void CreateConnection();
        void PublishEventMessage(EventMessage eventMessage);
        void CreateQueueWithTopics(string queueName, IEnumerable<string> topicExpressions);
        void StartReceiving(string queueName, EventReceivedCallback callback);
        void Dispose();
    }

    public delegate void EventReceivedCallback(EventMessage eventMessage);
}