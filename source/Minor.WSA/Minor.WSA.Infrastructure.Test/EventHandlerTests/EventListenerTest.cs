using Minor.WSA.Infrastructure;
using Minor.WSA.Infrastructure.Test;
using Minor.WSA.Infrastructure.Test.EventHandlerTests;
using Moq;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using RabbitMQ.Client.Events;

public class EventListenerTest
{
    [Fact]
    public void EventIsDispatched()
    {
        // Arrange
        var testHandler = new DispatcherTestMock();
        var factoryMock = new Mock<IFactory>();
        factoryMock.Setup(fm => fm.GetInstance()).Returns(testHandler);

        var factory = factoryMock.Object;
        var method = typeof(DispatcherTestMock).GetMethod("HandleDispatchTestEvent");
        var paramType = typeof(DispatchTestEvent);

        var dispatchers = new Dictionary<string, EventDispatcher>();
        dispatchers.Add("MVM.Test.DispatchTest", new EventDispatcher(factory, method, paramType));

        var busOptions = new BusOptions(exchangeName: "EventListenerTest_Ex01");
        using (var connection = RabbitTestHelp.CreateFactoryFrom(busOptions).CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.ExchangeDeclare("EventListenerTest_Ex01", ExchangeType.Topic, durable: false, autoDelete: false);

            var target = new EventListener("EventListenerTest_Q01", dispatchers);
            target.OpenEventQueue(channel, "EventListenerTest_Ex01");
            target.StartProcessing();

            var evt = new DispatchTestEvent { Number = 7 };
            using (var publisher = new EventPublisher(busOptions))
            {
                // Act
                publisher.Publish(evt);
            }

            // Assert
            Thread.Sleep(100);
            Assert.NotNull(testHandler.EventReceived);
            Assert.Equal(7, testHandler.EventReceived.Number);
        }

        RabbitTestHelp.DeleteExchange(busOptions);
    }

    [Fact]
    public void EventWithDifferentRoutingKeyIs_NOT_Dispatched()
    {
        // Arrange
        var testHandler = new DispatcherTestMock();
        var factoryMock = new Mock<IFactory>();
        factoryMock.Setup(fm => fm.GetInstance()).Returns(testHandler);

        var factory = factoryMock.Object;
        var method = typeof(DispatcherTestMock).GetMethod("HandleDispatchTestEvent");
        var paramType = typeof(DispatchTestEvent);

        var dispatchers = new Dictionary<string, EventDispatcher>();
        dispatchers.Add("Wrong.RoutingKey", new EventDispatcher(factory, method, paramType));

        var busOptions = new BusOptions(exchangeName: "EventListenerTest_Ex02");
        using (var connection = RabbitTestHelp.CreateFactoryFrom(busOptions).CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.ExchangeDeclare("EventListenerTest_Ex02", ExchangeType.Topic, durable: false, autoDelete: false);

            var target = new EventListener("EventListenerTest_Q02", dispatchers);
            target.OpenEventQueue(channel, "EventListenerTest_Ex02");
            target.StartProcessing();

            var evt = new DispatchTestEvent { Number = 7 };
            using (var publisher = new EventPublisher(busOptions))
            {
                // Act
                publisher.Publish(evt);
            }

            // Assert
            Thread.Sleep(100);
            Assert.Null(testHandler.EventReceived);
        }

        RabbitTestHelp.DeleteExchange(busOptions);
    }

    [Fact]
    public void EventAreProcessedThatArePublished_AFTER_OpenEventQueue_but_BEFORE_StartProcessing()
    {
        // Arrange
        var testHandler = new DispatcherTestMock();
        var factoryMock = new Mock<IFactory>();
        factoryMock.Setup(fm => fm.GetInstance()).Returns(testHandler);

        var factory = factoryMock.Object;
        var method = typeof(DispatcherTestMock).GetMethod("HandleDispatchTestEvent");
        var paramType = typeof(DispatchTestEvent);

        var dispatchers = new Dictionary<string, EventDispatcher>();
        dispatchers.Add("MVM.Test.DispatchTest", new EventDispatcher(factory, method, paramType));

        var busOptions = new BusOptions(exchangeName: "EventListenerTest_Ex03");
        using (var connection = RabbitTestHelp.CreateFactoryFrom(busOptions).CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.ExchangeDeclare("EventListenerTest_Ex03", ExchangeType.Topic, durable: false, autoDelete: false);

            var target = new EventListener("EventListenerTest_Q03", dispatchers);
            target.OpenEventQueue(channel, "EventListenerTest_Ex03");

            var evt = new DispatchTestEvent { Number = 7 };
            using (var publisher = new EventPublisher(busOptions))
            {
                publisher.Publish(evt);
            }
            Thread.Sleep(100);

            // Act
            target.StartProcessing();

            // Assert
            Thread.Sleep(100);
            Assert.NotNull(testHandler.EventReceived);
            Assert.Equal(7, testHandler.EventReceived.Number);
        }

        RabbitTestHelp.DeleteExchange(busOptions);
    }

}