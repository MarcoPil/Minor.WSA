﻿using System.Collections.Generic;
using RabbitMQ.Client;

namespace Minor.WSA.Infrastructure
{
    public interface IEventListener
    {
        string QueueName { get; }
        IEnumerable<string> RoutingKeyExpressions { get; }

        void OpenEventQueue(IModel channel, string exchangeName);
        void StartProcessing();
    }
}