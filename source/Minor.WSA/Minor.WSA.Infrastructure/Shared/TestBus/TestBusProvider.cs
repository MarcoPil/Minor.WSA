﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public void PublishEvent(EventMessage eventMessage)
        {
            LoggedMessages.Add(eventMessage);
            foreach (var eventQueue in _namedQueueus.Values)
            {
                eventQueue.EnqueueIfMatches(eventMessage.RoutingKey, eventMessage);
            }
        }

        public void StartReceivingEvents(string queueName, EventReceivedCallback callback)
        {
            _namedQueueus[queueName].StartDequeueing(callback);
        }

        public void CreateQueue(string queueName)
        {
            throw new NotImplementedException();
        }

        public Task<CommandResponseMessage> SendCommandAsync(CommandRequestMessage command)
        {
            throw new NotImplementedException();
        }

        public void StartReceivingCommands(string queueName, CommandReceivedCallback callback)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}
