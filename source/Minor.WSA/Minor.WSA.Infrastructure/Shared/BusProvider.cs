using RabbitMQ.Client;
using System;
using System.Text;
using System.Collections.Generic;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;
using System.Threading;
using Minor.WSA.Infrastructure.Shared;

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

        public void PublishEvent(EventMessage eventMessage)
        {
            // set metadata
            var props = Channel.CreateBasicProperties();
            props.Timestamp = new AmqpTimestamp(eventMessage.Timestamp);
            props.CorrelationId = eventMessage.CorrelationId;
            props.Type = eventMessage.EventType;
            // set payload
            var buffer = Encoding.UTF8.GetBytes(eventMessage.JsonMessage);
            // publish event
            Channel.BasicPublish(exchange: busOptions.ExchangeName,
                                     routingKey: eventMessage.RoutingKey,
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

        public void StartReceivingEvents(string queueName, EventReceivedCallback callback)
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

        public void CreateQueue(string queueName)
        {
            throw new NotImplementedException();
        }
        public Task<CommandResponseMessage> SendCommandAsync(CommandRequestMessage command)
        {
            // set up for receiving the response
            var replyQueueName = Channel.QueueDeclare().QueueName;
            var correlationId = Guid.NewGuid().ToString();
            var receiveHandle = new AutoResetEvent(false);
            CommandResponseMessage commandResponseMessage = null;

            var consumer = new EventingBasicConsumer(Channel);
            consumer.Received += (model, ea) =>
            {
                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    commandResponseMessage = new CommandResponseMessage(
                        callbackQueueName: replyQueueName,
                        correlationId: correlationId,
                        jsonMessage: Encoding.UTF8.GetString(ea.Body)
                    );
                    receiveHandle.Set();
                }
            };
            Channel.BasicConsume(queue: replyQueueName, autoAck: true, consumer: consumer);

            // set metadata
            IBasicProperties requestProps = Channel.CreateBasicProperties();
            requestProps.CorrelationId = correlationId;
            requestProps.ReplyTo = replyQueueName;
            requestProps.Type = command.CommandType;
            // set payload
            var buffer = Encoding.UTF8.GetBytes(command.JsonMessage);
            // send command
            Channel.BasicPublish(exchange: "",
                                 routingKey: command.ServiceQueueName,
                                 basicProperties: requestProps,
                                 body: buffer);

            return Task<CommandResponseMessage>.Factory.StartNew(() =>
            {
                receiveHandle.WaitOne();
                return commandResponseMessage;
            });
        }

        public void StartReceivingCommands(string queueName, CommandReceivedCallback callback)
        {
            Channel.QueueDeclare(queue: queueName, durable: false, exclusive: false,
                                 autoDelete: false, arguments: null);
            Channel.BasicQos(0, 1, false);
            var consumer = new EventingBasicConsumer(Channel);
            Channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

            consumer.Received += (model, ea) =>
            {
                // receive command
                var receivedProps = ea.BasicProperties;
                var commandReceivedMessage = new CommandReceivedMessage(
                    callbackQueueName: receivedProps.ReplyTo,
                    correlationId: receivedProps.CorrelationId,
                    commandType: receivedProps.Type,
                    jsonMessage: Encoding.UTF8.GetString(ea.Body)
                );

                // execute command
                var commandResponse = callback(commandReceivedMessage); 

                // send response
                IBasicProperties responseProps = Channel.CreateBasicProperties();
                responseProps.CorrelationId = receivedProps.CorrelationId;
                var buffer = Encoding.UTF8.GetBytes(commandResponse.JsonMessage);
                Channel.BasicPublish(exchange: "",
                                     routingKey: receivedProps.ReplyTo,
                                     basicProperties: responseProps,
                                     body: buffer);

                // send acknowledgement
                Channel.BasicAck(ea.DeliveryTag, false);     
            };
        }

        public void Dispose()
        {
            Channel?.Dispose();
            _connection?.Dispose();
        }

    }
}