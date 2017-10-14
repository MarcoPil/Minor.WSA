using RabbitMQ.Client;
using System;
using System.Text;
using System.Collections.Generic;
using RabbitMQ.Client.Events;

namespace Minor.WSA.Infrastructure
{
    public class BusProvider : IDisposable, IBusProvider
    {
        private IConnection _connection;
        protected IModel Channel;
        private BusOptions busOptions;

        public BusProvider(BusOptions busOptions)
        {
            this.busOptions = busOptions;
        }
        public void CreateConnection()
        {
            var factory = new ConnectionFactory()
            {
                HostName = busOptions.HostName,
                Port = busOptions.Port,
                UserName = busOptions.UserName,
                Password = busOptions.Password,
            };
            try
            {
                _connection = factory.CreateConnection();
            }
            catch
            {
                throw new MicroserviceConfigurationException("The Eventbus (RabbitMQ service) cannot be reached.");
            }

            Channel = _connection.CreateModel();

            Channel.ExchangeDeclare(exchange: busOptions.ExchangeName,
                                    type: ExchangeType.Topic,
                                    durable: false, autoDelete: false, arguments: null);
        }

        public void PublishRawMessage(long timestamp, string routingKey, string correlationId, string eventType, string jsonMessage)
        {
            // set metadata
            var props = Channel.CreateBasicProperties();
            props.Timestamp = new AmqpTimestamp(timestamp);
            props.CorrelationId = correlationId;
            props.Type = eventType;
            // set payload
            var buffer = Encoding.UTF8.GetBytes(jsonMessage);
            // publish event
            Channel.BasicPublish(exchange: busOptions.ExchangeName,
                                     routingKey: routingKey,
                                     basicProperties: props,
                                     body: buffer);
        }

        public void CreateQueueWithTopics(string queueName, IEnumerable<string> topicExpressions)
        {
            // declare queue
            // should be NO-auto delete queue,  (queue must survive the listener-process, so that no event gets lost.
            Channel.QueueDeclare(queue: queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            // do queue-bind for all routingkey expressions
            foreach (var topicExpr in topicExpressions)
            {
                Channel.QueueBind(queue: queueName,
                                  exchange: busOptions.ExchangeName,
                                  routingKey: topicExpr,
                                  arguments: null);
            }

        }

        public void StartReceiving(string queueName, EventReceivedCallback callback)
        {
            // register a BasicComsume WITH acknoledgement (only events that have been processed me be removed)
            var consumer = new EventingBasicConsumer(Channel);
            consumer.Received += (object sender, BasicDeliverEventArgs e) =>
            {
                try
                {
                    var props = e.BasicProperties;

                    var eventMessage = new EventMessage(
                        timestamp: props.Timestamp.UnixTime, 
                        routingKey: e.RoutingKey,
                        correlationId: props.CorrelationId,
                        eventType: props.Type,
                        jsonMessage: Encoding.UTF8.GetString(e.Body)    // fetch payload
                    );

                    callback(eventMessage); //process event

                    Channel.BasicAck(e.DeliveryTag, false);     // send acknowledgement
                }
                catch
                {
                    // Fail silently...
                    // but if something goes wrong in dispatching the event, then no acknoledgement is sent.
                }
            };
            Channel.BasicConsume(queue: queueName,
                                  autoAck: true,
                                  consumer: consumer);
        }


        public void Dispose()
        {
            Channel?.Dispose();
            _connection?.Dispose();
        }
    }
}