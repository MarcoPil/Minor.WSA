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

        var dispatchers = new Dictionary<string, IEventDispatcher>();
        dispatchers.Add("MVM.Test.DispatchTest", new EventDispatcher(factory, method, paramType));

        var busOptions = new BusOptions(exchangeName: "EventListenerTest_Ex01");
        busOptions.Provider.CreateConnection();

        try
        {
            var target = new EventListener("EventListenerTest_Q01", dispatchers);
            target.OpenEventQueue(busOptions);
            target.StartHandling();

            var evt = new DispatchTestEvent { Number = 7 };
            var publishBusOptions = new BusOptions(exchangeName: "EventListenerTest_Ex01");
            using (var publisher = new EventPublisher(publishBusOptions))
            {
                // Act
                publisher.Publish(evt);
            }

            // Assert
            Thread.Sleep(100);
            Assert.NotNull(testHandler.EventReceived);
            Assert.Equal(7, testHandler.EventReceived.Number);
        }
        finally
        {
            busOptions.Provider.Dispose();
        }
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

        var dispatchers = new Dictionary<string, IEventDispatcher>();
        dispatchers.Add("Wrong.RoutingKey", new EventDispatcher(factory, method, paramType));

        var busOptions = new BusOptions(exchangeName: "EventListenerTest_Ex02");
        busOptions.Provider.CreateConnection();

        try
        {
            var target = new EventListener("EventListenerTest_Q02", dispatchers);
            target.OpenEventQueue(busOptions);
            target.StartHandling();

            var evt = new DispatchTestEvent { Number = 7 };
            var publishBusOptions = new BusOptions(exchangeName: "EventListenerTest_Ex02");
            using (var publisher = new EventPublisher(publishBusOptions))
            {
                // Act
                publisher.Publish(evt);
            }

            // Assert
            Thread.Sleep(100);
            Assert.Null(testHandler.EventReceived);
        }
        finally
        {
            busOptions.Provider.Dispose();
        }
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

        var dispatchers = new Dictionary<string, IEventDispatcher>();
        dispatchers.Add("MVM.Test.DispatchTest", new EventDispatcher(factory, method, paramType));

        var busOptions = new BusOptions(exchangeName: "EventListenerTest_Ex03");
        busOptions.Provider.CreateConnection();

        try
        {
            var target = new EventListener("EventListenerTest_Q03", dispatchers);
            target.OpenEventQueue(busOptions);

            var evt = new DispatchTestEvent { Number = 7 };
            var publishBusOptions = new BusOptions(exchangeName: "EventListenerTest_Ex03");
            using (var publisher = new EventPublisher(publishBusOptions))
            {
                publisher.Publish(evt);
            }
            Thread.Sleep(100);

            // Act
            target.StartHandling();

            // Assert
            Thread.Sleep(100);
            Assert.NotNull(testHandler.EventReceived);
            Assert.Equal(7, testHandler.EventReceived.Number);
        }
        finally
        {
            busOptions.Provider.Dispose();
        }
    }

    [Fact]
    public void EventIsHandledByMultipleHandlingMethods()
    {
        // Arrange
        var testHandler = new DispatcherTestMock();
        var factoryMock = new Mock<IFactory>();
        factoryMock.Setup(fm => fm.GetInstance()).Returns(testHandler);

        var factory = factoryMock.Object;
        var method = typeof(DispatcherTestMock).GetMethod("HandleDispatchTestEvent");
        var paramType = typeof(DispatchTestEvent);

        var dispatchers = new Dictionary<string, IEventDispatcher>();
        dispatchers.Add("*.Test.DispatchTest", new EventDispatcher(factory, method, paramType));
        dispatchers.Add("MVM.*.DispatchTest", new EventDispatcher(factory, method, paramType));

        var busOptions = new BusOptions(exchangeName: "EventListenerTest_Ex04");
        busOptions.Provider.CreateConnection();

        try
        {
            var target = new EventListener("EventListenerTest_Q04", dispatchers);
            target.OpenEventQueue(busOptions);
            target.StartHandling();

            var evt = new DispatchTestEvent { Number = 7 };
            var publishBusOptions = new BusOptions(exchangeName: "EventListenerTest_Ex04");
            using (var publisher = new EventPublisher(publishBusOptions))
            {
                // Act - 
                publisher.Publish(evt); // publish the event once
            }

            // Assert
            Thread.Sleep(100);
            Assert.Equal(2, testHandler.ReceiveCount); // receive the event twice
        }
        finally
        {
            busOptions.Provider.Dispose();
        }
    }
}