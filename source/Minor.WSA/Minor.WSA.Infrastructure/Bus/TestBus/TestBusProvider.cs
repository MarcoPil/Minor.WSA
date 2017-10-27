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
        private Dictionary<string, TestCommandQueue> _namedCommandQueues;
        public IEnumerable<TestCommandQueue> CommandQueues => _namedCommandQueues.Values;

        public List<EventMessage> LoggedEventMessages { get; }
        public List<CommandRequestMessage> LoggedCommandRequestMessages { get; }

        public TestBusProvider()
        {
            _namedEventQueues = new Dictionary<string, TestEventQueue>();
            _namedCommandQueues = new Dictionary<string, TestCommandQueue>();
            LoggedEventMessages = new List<EventMessage>();
            LoggedCommandRequestMessages = new List<CommandRequestMessage>();
        }
        public void CreateConnection()
        {
        }

        /// <summary>
        /// Create Queue for receiving events. 
        /// If an event is Published with a routing key that matches one of the topicExpressions,
        /// the Event is added to the queue.
        /// </summary>
        /// <param name="queueName">Name of the queue for registering events</param>
        /// <param name="topicExpressions">all events having a routing key that matches one of these topicexpressions is added to the queue</param>
        public void CreateQueueWithTopics(string queueName, IEnumerable<string> topicExpressions)
        {
            var eventQueue = new TestEventQueue(queueName, topicExpressions);
            _namedEventQueues.Add(queueName, eventQueue);
        }

        /// <summary>
        /// Events are broadcasted. They are added to each queue that has a topicexpression that matches the RoutingKey in the EventMessage.
        /// All published events are logged in LoggedEventMessages.
        /// </summary>
        /// <param name="eventMessage">Should at least contain a routing key</param>
        public void PublishEvent(EventMessage eventMessage)
        {
            LoggedEventMessages.Add(eventMessage);
            foreach (var eventQueue in _namedEventQueues.Values)
            {
                eventQueue.EnqueueIfMatches(eventMessage.RoutingKey, eventMessage);
            }
        }

        /// <summary>
        /// Handles all events that are broadcasted to a specific event-queue
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="callback"></param>
        public void StartReceivingEvents(string queueName, EventReceivedCallback callback)
        {
            _namedEventQueues[queueName].StartDequeueing(callback);
        }

        /// <summary>
        /// Create Queue for receiving commands. 
        /// If and only if a command is sent to this specific queue, it is added.
        /// </summary>
        /// <param name="queueName"></param>
        public void CreateCommandQueue(string queueName)
        {
            var testCommandQueue = new TestCommandQueue(queueName);
            _namedCommandQueues.Add(queueName, testCommandQueue);
        }

        public Task<CommandResponseMessage> SendCommandAsync(CommandRequestMessage command)
        {
            LoggedCommandRequestMessages.Add(command);
            if (_namedCommandQueues.ContainsKey(command.ServiceQueueName))
            {
                _namedCommandQueues[command.ServiceQueueName].SendCommandAsync(command);
            }
            return null;
        }

        public void StartReceivingCommands(string queueName, CommandReceivedCallback callback)
        {
            if (_namedCommandQueues.ContainsKey(queueName))
            {
                _namedCommandQueues[queueName].Register(callback);
            }
            else
            {
                throw new MicroserviceException($"Cannot .StartReceivingCommands() on a non-existing queue. Queue with name '{queueName}' does not exist. Consider calling .CreateCommandQueue(\"{queueName}\") first.");
            }
        }

        public void Dispose()
        {
        }
    }
}
