using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minor.WSA.Infrastructure.TestBus
{
    public class TestQueue
    {
        private bool _isQueueing;
        public string QueueName { get; }
        public List<string> Topics { get; }
        public Queue<TestQueueMessage> Messages { get; }
        public TestQueueCallback Callbacks;

        public TestQueue(string queueName) : this(queueName, new string[] { queueName }) { }
        public TestQueue(string queueName, IEnumerable<string> topicExpressions)
        {
            _isQueueing = true;
            Callbacks = null;
            QueueName = queueName;
            Topics = topicExpressions.ToList();
            Messages = new Queue<TestQueueMessage>();
        }

        public void BasicPublish(TestQueueMessage message)
        {
            if (Topics.ThatMatch(message.RoutingKey).Any())
            {
                if (_isQueueing)
                {
                    Messages.Enqueue(message);
                }
                else
                {
                    Callbacks.Invoke(message);
                }
            }
        }

        public void BasicConsume(TestQueueCallback callback)
        {
            Callbacks += callback;
            if (_isQueueing)
            {
                while (Messages.Any())
                {
                    var message = Messages.Dequeue();
                    Callbacks.Invoke(message);
                }
                _isQueueing = false;
            }
        }
    }

    public delegate void TestQueueCallback(TestQueueMessage eventMessage);

}
