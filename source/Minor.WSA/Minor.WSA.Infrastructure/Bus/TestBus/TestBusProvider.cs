using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minor.WSA.Infrastructure.Shared.TestBus
{
    public class TestBusProvider : IBusProvider
    {
        private Dictionary<string, TestEventQueue> _namedEventQueues;

        public List<EventMessage> LoggedEventMessages { get; }

        public TestBusProvider()
        {
            _namedEventQueues = new Dictionary<string, TestEventQueue>();
            LoggedEventMessages = new List<EventMessage>();
        }
        public void CreateConnection()
        {
        }

        public void CreateQueueWithTopics(string queueName, IEnumerable<string> topicExpressions)
        {
            var eventQueue = new TestEventQueue(queueName, topicExpressions);
            _namedEventQueues.Add(queueName, eventQueue);
        }
        public void PublishEvent(EventMessage eventMessage)
        {
            LoggedEventMessages.Add(eventMessage);
            foreach (var eventQueue in _namedEventQueues.Values)
            {
                eventQueue.EnqueueIfMatches(eventMessage.RoutingKey, eventMessage);
            }
        }

        public void StartReceivingEvents(string queueName, EventReceivedCallback callback)
        {
            _namedEventQueues[queueName].StartDequeueing(callback);
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
