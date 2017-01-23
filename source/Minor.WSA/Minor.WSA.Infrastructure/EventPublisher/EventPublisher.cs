using System;
using Minor.WSA.Common;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Minor.WSA.Infrastructure
{
    /// <summary>
    /// An EventPublisher publishes a domain event on the event bus (configured by the BusOptions).
    /// Each EventPublisher creates its own connection to rabbitMQ.
    /// </summary>
    public class EventPublisher : EventBusBase, IEventPublisher
    {
        /// <summary>
        /// Each EventPublisher creates its own connection to rabbitMQ.
        /// </summary>
        /// <param name="options">the configuration of the RabbitMQ connection. If none are passed, the default BusOptions are being used.</param>
        public EventPublisher(BusOptions options = default(BusOptions)) : base(options)
        {
            try
            {
                CreateConnection();
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        /// <summary>
        /// Publishes a domain event on the event bus (configured by the BusOptions).
        /// Make sure that the appropriate Routing Key has been set in the DomainEvent.
        /// </summary>
        /// <param name="domainEvent">The domain event to be published. </param>
        public void Publish(DomainEvent domainEvent)
        {
            PublishRawMessage(
                timestamp:     domainEvent.Timestamp, 
                routingKey:    domainEvent.RoutingKey,
                correlationId: domainEvent.ID.ToString(),
                eventType:     domainEvent.GetType().FullName, 
                jsonMessage:   JsonConvert.SerializeObject(domainEvent)
            );
        }

        private void PublishRawMessage(long timestamp, string routingKey, string correlationId, string eventType, string jsonMessage)
        {
            // set metadata
            var props = Channel.CreateBasicProperties();
            props.Timestamp = new AmqpTimestamp(timestamp);
            props.CorrelationId = correlationId;
            props.Type = eventType;
            // set payload
            var buffer = Encoding.UTF8.GetBytes(jsonMessage);
            // publish event
            Channel.BasicPublish(exchange: BusOptions.ExchangeName,
                                     routingKey: routingKey,
                                     basicProperties: props,
                                     body: buffer);
        }
    }
}