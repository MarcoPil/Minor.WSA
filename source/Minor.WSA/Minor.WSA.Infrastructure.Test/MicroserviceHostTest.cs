using Minor.WSA.Infrastructure;
using Minor.WSA.Infrastructure.TestBus;
using Minor.WSA.Infrastructure.Test;
using Minor.WSA.Infrastructure.Test.EventHandlerTests;
using Moq;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

public class MicroserviceHostTest
{
    [Fact]
    public void DefaultBusoptions()
    {
        using (var target = new MicroserviceHost(null, null, default(BusOptions)))
        {
            var result = target.BusOptions;

            Assert.Equal("WSA.DefaultEventBus", result.ExchangeName);
            Assert.Equal("localhost", result.HostName);
            Assert.Equal(5672, result.Port);
            Assert.Equal("guest", result.UserName);
            Assert.Equal("guest", result.Password);
        }
        RabbitTestHelp.DeleteExchange(new BusOptions());
    }

    [Fact]
    public void SetsBusoptions()
    {
        var busOptions = new BusOptions(hostName: "127.0.0.1");
        using (var target = new MicroserviceHost(null, null, busOptions))
        {
            var result = target.BusOptions;

            Assert.Equal("WSA.DefaultEventBus", result.ExchangeName);
            Assert.Equal("127.0.0.1", result.HostName);
            Assert.Equal(5672, result.Port);
            Assert.Equal("guest", result.UserName);
            Assert.Equal("guest", result.Password);
        }
    }

    [Fact]
    public void StartListening_CallsOpenEventQueueOnListeners()
    {
        var mock1 = new Mock<IEventListener>(MockBehavior.Loose);
        var mock2 = new Mock<IEventListener>(MockBehavior.Loose);
        var eventListeners = new List<IEventListener> { mock1.Object, mock2.Object };
        var controllers = new List<IController>();

        var busOptions = new TestBusOptions();
        using (var target = new MicroserviceHost(eventListeners, controllers, busOptions))
        {
            // Act
            target.StartListening();

            // Assert
            mock1.Verify(el => el.OpenEventQueue(busOptions), Times.Once);
            mock2.Verify(el => el.OpenEventQueue(busOptions), Times.Once);
        }
    }
    [Fact]
    public void StartListening_CallsOpenCommandQueueOnControllers()
    {
        var eventListeners = new List<IEventListener>();
        var mock1 = new Mock<IController>(MockBehavior.Loose);
        var mock2 = new Mock<IController>(MockBehavior.Loose);
        var controllers = new List<IController>() { mock1.Object, mock2.Object };

        var busOptions = new TestBusOptions();
        using (var target = new MicroserviceHost(eventListeners, controllers, busOptions))
        {
            // Act
            target.StartListening();

            // Assert
            mock1.Verify(el => el.OpenCommandQueue(busOptions), Times.Once);
            mock2.Verify(el => el.OpenCommandQueue(busOptions), Times.Once);
        }

    }

    [Fact]
    public void StartHandling_CallsStartHandlingOnListeners()
    {
        var mock1 = new Mock<IEventListener>(MockBehavior.Loose);
        var mock2 = new Mock<IEventListener>(MockBehavior.Loose);
        var eventListeners = new List<IEventListener> { mock1.Object, mock2.Object };
        var controllers = new List<IController>();

        var busOptions = new TestBusOptions();
        using (var target = new MicroserviceHost(eventListeners, controllers, busOptions))
        {
            target.StartListening();

            // Act
            target.StartHandling();

            // Assert
            mock1.Verify(el => el.StartHandling(), Times.Once);
            mock2.Verify(el => el.StartHandling(), Times.Once);
        }
    }


    [Fact]
    public void StartHandling_CallsStartHandlingOnControllers()
    {
        var eventListeners = new List<IEventListener>();
        var mock1 = new Mock<IController>(MockBehavior.Loose);
        var mock2 = new Mock<IController>(MockBehavior.Loose);
        var controllers = new List<IController> { mock1.Object, mock2.Object };

        var busOptions = new TestBusOptions();
        using (var target = new MicroserviceHost(eventListeners, controllers, busOptions))
        {
            target.StartListening();

            // Act
            target.StartHandling();

            // Assert
            mock1.Verify(el => el.StartHandling(), Times.Once);
            mock2.Verify(el => el.StartHandling(), Times.Once);
        }
    }

    [Fact]
    public void DispatchesEvents()
    {
        // Arrange
        var testHandler = new DispatcherTestMock();
        var factoryMock = new Mock<IFactory>();
        factoryMock.Setup(fm => fm.GetInstance()).Returns(testHandler);
        var factory = factoryMock.Object;
        var method = typeof(DispatcherTestMock).GetMethod("HandleDispatchTestEvent");
        var paramType = typeof(DispatchTestEvent);
        var dispatcher = new EventDispatcher(factory, method, paramType);
    }

    [Fact]
    public void CannotStartHandlingBeforeStartListening()
    {
        var eventListeners = new List<IEventListener>();
        var controllers = new List<Controller>();

        var busOptions = new BusOptions(exchangeName: "MicroserviceHostTest03");
        using (var target = new MicroserviceHost(eventListeners, controllers, busOptions))
        {
            // Act
            Action action = () => target.StartHandling();

            // Assert
            var ex = Assert.Throws<MicroserviceException>(action);
            Assert.Equal("A MicroserviceHost can only start handling after start listening. Consider calling .StartListening() first.", ex.Message);
        }

        RabbitTestHelp.DeleteExchange(busOptions);
    }
}