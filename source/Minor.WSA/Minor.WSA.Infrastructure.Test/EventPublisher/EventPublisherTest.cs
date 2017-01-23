using Minor.WSA.Common;
using Minor.WSA.Infrastructure;
using Minor.WSA.Infrastructure.Test;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class EventPublisherTest
{
    [Fact]
    public void PublishEmitsEventOnExhange()
    {
        // Arrange (set the stage)
        var factory = new ConnectionFactory();
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.ExchangeDeclare(exchange: "TestEventbus",
                                    type: ExchangeType.Topic);
            var listenQueueName = channel.QueueDeclare().QueueName;

            channel.QueueBind(queue: listenQueueName,
                              exchange: "TestEventbus",
                              routingKey: "Minor.WSA.PublisherTestEvent");

            var eventEmitted = false;
            BasicDeliverEventArgs receivedEvent = null;
            var handle = new AutoResetEvent(false);
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (object sender, BasicDeliverEventArgs e) =>
            {
                eventEmitted = true;
                receivedEvent = e;
                handle.Set();
            };

            channel.BasicConsume(queue: listenQueueName,
                                 noAck: true,
                                 consumer: consumer);

            // Arrange
            var options = new BusOptions(exchangeName: "TestEventbus");
            using (IEventPublisher target = new EventPublisher(options))
            {
                DomainEvent sendEvent = new PublisherTestEvent();

                // Act
                target.Publish(sendEvent);

                // Assert
                if (handle.WaitOne(2000))
                {
                    // Assert that Event has been raised:
                    Assert.True(eventEmitted);
                    Assert.NotNull(receivedEvent);
                    // Assert that EventTimestamp has been sent correctly:
                    Assert.Equal(new AmqpTimestamp(sendEvent.Timestamp), receivedEvent.BasicProperties.Timestamp);
                    // Assert that EventID has been sent correctly:
                    Assert.Equal(sendEvent.ID.ToString(), receivedEvent.BasicProperties.CorrelationId);
                    // Assert that EventType has been sent correctly:
                    Assert.Equal("Minor.WSA.Infrastructure.Test.PublisherTestEvent", receivedEvent.BasicProperties.Type);
                    // Assert that Event Payload has been sent correctly:
                    string jsonMessage = Encoding.UTF8.GetString(receivedEvent.Body);
                    Assert.Equal($"{{\"RoutingKey\":\"Minor.WSA.PublisherTestEvent\",\"Timestamp\":{sendEvent.Timestamp},\"ID\":\"{sendEvent.ID}\"}}", jsonMessage);
                }
                else
                {
                    Assert.True(false, "Event has not been received before time out (2000ms)");
                }
            }
        }
    }

    [Fact]
    public void DefaultBusoptions()
    {
        using (var target = new EventPublisher())
        {
            var result = target.BusOptions;

            Assert.Equal("WSA.DefaultEventBus", result.ExchangeName);
            Assert.Equal("localhost", result.HostName);
            Assert.Equal(5672, result.Port);
            Assert.Equal("guest", result.UserName);
            Assert.Equal("guest", result.Password);
        }
    }

    [Fact]
    public void CustomBusoptions()
    {
        var options = new BusOptions(hostName: "127.0.0.1");
        using (var target = new EventPublisher(options))
        {
            var result = target.BusOptions;

            Assert.Equal("127.0.0.1", result.HostName);
        }
    }

    [Fact]
    public void HostCannotBeReached()
    {
        var options = new BusOptions(hostName: "CannotBeReached");

        Action action = () => { var target = new EventPublisher(options); };

        Assert.Throws<MicroserviceConfigurationException>(action);
    }
}