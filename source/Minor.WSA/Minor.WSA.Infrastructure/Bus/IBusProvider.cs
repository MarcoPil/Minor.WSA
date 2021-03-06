﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Minor.WSA.Infrastructure
{
    public interface IBusProvider
    {
        void CreateConnection();

        void PublishEvent(EventMessage eventMessage);
        void CreateQueueWithTopics(string queueName, IEnumerable<string> topicExpressions);
        void StartReceivingEvents(string queueName, EventReceivedCallback callback);

        Task<CommandResponseMessage> SendCommandAsync(CommandRequestMessage command);
        void CreateCommandQueue(string queueName);
        void StartReceivingCommands(string queueName, CommandReceivedCallback callback);
        void Dispose();
    }

    public delegate void EventReceivedCallback(EventMessage eventMessage);
    public delegate CommandResultMessage CommandReceivedCallback(CommandReceivedMessage commandReceivedMessage);
}