using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Minor.WSA.Infrastructure.Shared.TestBus
{
    internal class TestEventQueue
    {
        private object _queueLock = new object();
        private bool _isQueueing;
        private EventReceivedCallback _callbacks;

        public string QueueName { get; }
        public List<string> Topics { get; }
        public Queue<EventMessage> EventMessages { get; }

        public TestEventQueue(string queueName, IEnumerable<string> topicExpressions)
        {
            _isQueueing = true;
            _callbacks = null;
            QueueName = queueName;
            Topics = topicExpressions.ToList();
            EventMessages = new Queue<EventMessage>();
        }

        public void EnqueueIfMatches(string routingKey, EventMessage eventMessage)
        {
            if (Topics.ThatMatch(routingKey).Any())
            {
                if (_isQueueing)
                {
                    EventMessages.Enqueue(eventMessage);
                }
                else
                {
                    _callbacks.Invoke(eventMessage);
                }
            }
        }

        public void StartDequeueing(EventReceivedCallback callback)
        {
            _callbacks += callback;
            if (_isQueueing)
            {
                while (EventMessages.Any())
                {
                    var eventMessage = EventMessages.Dequeue();
                    _callbacks.Invoke(eventMessage);
                }
                _isQueueing = false;
            }
        }
    }
}