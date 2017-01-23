using Minor.WSA.Common;
using Minor.WSA.Infrastructure;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
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
                              routingKey: "TestEvent");

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
                    Assert.True(eventEmitted);
                    Assert.NotNull(receivedEvent);
                    Assert.Equal(new AmqpTimestamp(sendEvent.Timestamp), receivedEvent.BasicProperties.Timestamp);
                    Assert.Equal("InfoSupport.WSA.Infrastructure.Test.TestEvent", receivedEvent.BasicProperties.Type);
                }
                else
                {
                    Assert.True(false, "Event has not been received before time out (2000ms)");
                }
            }
        }
    }

    //[Fact]
    //public void DefaultBusoptions()
    //{
    //    using (var target = new EventPublisher())
    //    {
    //        var result = target.BusOptions;

    //        Assert.Equal("WSA.DefaultEventBus", result.ExchangeName);
    //        Assert.Equal(null, result.QueueName);
    //        Assert.Equal("localhost", result.HostName);
    //        Assert.Equal(5672, result.Port);
    //        Assert.Equal("guest", result.UserName);
    //        Assert.Equal("guest", result.Password);
    //    }
    //}

    //[Fact]
    //public void CustomBusoptions()
    //{
    //    var options = new BusOptions { HostName = "127.0.0.1" };
    //    using (var target = new EventPublisher(options))
    //    {
    //        var result = target.BusOptions;

    //        Assert.Equal("127.0.0.1", result.HostName);
    //    }
    //}
}