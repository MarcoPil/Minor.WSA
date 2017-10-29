using Minor.WSA.Infrastructure.TestBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Minor.WSA.Infrastructure.Shared.TestBus
{
    public class TestBusProvider : IBusProvider
    {
        private Dictionary<string, TestEventQueue> _namedEventQueues;
        private Dictionary<string, TestQueue> _namedQueues;
        public IEnumerable<TestQueue> Queues => _namedQueues.Values;

        public List<EventMessage> LoggedEventMessages { get; }
        public List<CommandRequestMessage> LoggedCommandRequestMessages { get; }

        public TestBusProvider()
        {
            _namedEventQueues = new Dictionary<string, TestEventQueue>();
            //_namedCommandQueues = new Dictionary<string, TestCommandQueue>();
            _namedQueues = new Dictionary<string, TestQueue>();
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
            if (!_namedQueues.ContainsKey(queueName))
            {
                var testQueue = new TestQueue(queueName);
                _namedQueues.Add(queueName, testQueue);
            }
        }

        public Task<CommandResponseMessage> SendCommandAsync(CommandRequestMessage command)
        {
            LoggedCommandRequestMessages.Add(command);

            CreateCommandQueue(command.ServiceQueueName);

            var callbackQueueName = Guid.NewGuid().ToString();
            var replyQueue = new TestQueue(callbackQueueName);
            _namedQueues.Add(callbackQueueName, replyQueue);
            var correlationId = Guid.NewGuid().ToString();

            CommandResponseMessage response = null;
            var receiveHandle = new AutoResetEvent(false);
            TestQueueCallback callback = (TestQueueMessage resultMessage) =>
            {
                if (correlationId == resultMessage.CorrelationId)
                {
                    response = new CommandResponseMessage(
                        callbackQueueName: resultMessage.ReplyTo,
                        correlationId: resultMessage.CorrelationId,
                        jsonMessage: resultMessage.JsonBody
                    );
                    receiveHandle.Set();
                }
            };
            replyQueue.BasicConsume(callback);

            var message = new TestQueueMessage(
               routingKey: command.ServiceQueueName,
               replyTo: callbackQueueName,
               correlationId: correlationId,
               type: command.CommandType,
               jsonBody: command.JsonMessage
            );
            _namedQueues[command.ServiceQueueName].BasicPublish(message);

            return Task<CommandResponseMessage>.Factory.StartNew(() =>
            {
                receiveHandle.WaitOne();
                return response;
            });
        }

        public void StartReceivingCommands(string queueName, CommandReceivedCallback callback)
        {
            if (_namedQueues.ContainsKey(queueName))
            {
                TestQueueCallback receivedCallback = (TestQueueMessage receivedMessage) =>
                {
                    var commandReceivedMessage = new CommandReceivedMessage(
                        callbackQueueName: receivedMessage.ReplyTo,
                        correlationId: receivedMessage.CorrelationId,
                        commandType: receivedMessage.Type,
                        jsonMessage: receivedMessage.JsonBody
                    );

                    CommandResultMessage commandResultMessage = callback(commandReceivedMessage);

                    var resultMessage = new TestQueueMessage(
                        routingKey: receivedMessage.ReplyTo,
                        correlationId: receivedMessage.CorrelationId,
                        type: commandResultMessage.Type,
                        jsonBody: commandResultMessage.JsonMessage
                    );
                    _namedQueues[receivedMessage.ReplyTo].BasicPublish(resultMessage);
                };
                _namedQueues[queueName].BasicConsume(receivedCallback);
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
