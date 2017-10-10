using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minor.WSA.Infrastructure
{
    /// <summary>
    /// For each EventHandler class (marked with the [EventHandler(queueName)]-attribute) and EventListener is created.
    /// This Eventlistener is responsible for receiving all events that arrive at this particular queue. Therefore 
    /// there can be no two EventListeners that listen to the same queue
    /// </summary>
    public class EventListener : IEventListener
    {
        private Dictionary<string, EventDispatcher> _dispatchers; //    string = routingkey-expression
        private IModel _channel;

        public string QueueName { get; }
        public IEnumerable<string> RoutingKeyExpressions => _dispatchers.Keys;

        public EventListener(string queueName, Dictionary<string, EventDispatcher> dispatchers)
        {
            QueueName = queueName;
            _dispatchers = dispatchers;
        }

        /// <summary>
        /// Open a named queue (QueueName), so that the same queue can be reused when an off-line application come back on-line,
        /// and bind it to the exchange over ALL registered routing key expressions (RoutingKeyExpressions).
        /// </summary>
        /// <param name="channel">An opened channel that represents a connection to an rabbitMQ service</param>
        /// <param name="exchangeName">The name of the topic-exchange to which the queue (QueueName) is bound - possibly multiple times, each time with a different routing key expression.</param>
        public virtual void OpenEventQueue(IModel channel, string exchangeName)
        {
            _channel = channel;
            // declare queue
            // should be NO-auto delete queue,  (queue must survive the listener-process, so that no event gets lost.
            channel.QueueDeclare(queue: QueueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            // do queue-bind for all routingkey expressions
            foreach (var routingKeyExpr in RoutingKeyExpressions)
            {
                channel.QueueBind(queue: QueueName,
                                  exchange: exchangeName,
                                  routingKey: routingKeyExpr,
                                  arguments: null);
            }

            // (from this moment in time, all relevant events are captured in the queue, for later processing)
        }

        /// <summary>
        /// Start listening to events
        /// </summary>
        public virtual void StartProcessing()
        {
            // register a BasicComsume WITH acknoledgement (only events that have been processed me be removed)
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += EventReceived;
            _channel.BasicConsume(queue: QueueName,
                                  autoAck: true,
                                  consumer: consumer);
        }

        protected virtual void EventReceived(object sender, BasicDeliverEventArgs e)
        {
            try
            {
                var matchingKeys = RoutingKeyMatcher.Match(e.RoutingKey, RoutingKeyExpressions);
                var jsonMessage = Encoding.UTF8.GetString(e.Body);  // fetch payload

                // process event
                foreach (string matchingKey in matchingKeys)
                {
                    var dispatcher = _dispatchers[matchingKey];
                    dispatcher.DispatchEvent(jsonMessage);
                }

                // send acknowledgement
                _channel.BasicAck(e.DeliveryTag, false);
            }
            catch
            {
                // Fail silently...
                // but if something goes wrong in dispatching the event, then no acknoledgement is sent.
            }
        }
    }
}
