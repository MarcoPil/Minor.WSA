using Minor.WSA.Infrastructure;
using Minor.WSA.Infrastructure.Shared;
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
    public void SetsBusoptions()
    {
        var busOptions = new BusOptions(hostName: "127.0.0.1");
        using (var target = new MicroserviceHost(null, busOptions))
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
    public void Open_CallsOpenEventQueueOnOlisteners()
    {
        var mock1 = new Mock<IEventListener>(MockBehavior.Loose);
        var mock2 = new Mock<IEventListener>(MockBehavior.Loose);
        var eventListeners = new List<IEventListener> { mock1.Object, mock2.Object };

        var busOptions = new BusOptions(exchangeName: "MicroserviceHostTest01");
        using (var target = new MicroserviceHost(eventListeners, busOptions))
        {
            // Act
            target.StartListening();

            // Assert
            mock1.Verify(el => el.OpenEventQueue(It.IsAny<BusOptions>()), Times.Once);
            mock2.Verify(el => el.OpenEventQueue(It.IsAny<BusOptions>()), Times.Once);
        }

        RabbitTestHelp.DeleteExchange(busOptions);
    }


    [Fact]
    public void Start_CallsOpenEventQueueOnOlisteners()
    {
        var mock1 = new Mock<IEventListener>(MockBehavior.Loose);
        var mock2 = new Mock<IEventListener>(MockBehavior.Loose);
        var eventListeners = new List<IEventListener> { mock1.Object, mock2.Object };

        var busOptions = new BusOptions(exchangeName: "MicroserviceHostTest02");
        using (var target = new MicroserviceHost(eventListeners, busOptions))
        {
            target.StartListening();

            // Act
            target.StartHandling();

            // Assert
            mock1.Verify(el => el.StartHandling(), Times.Once);
            mock2.Verify(el => el.StartHandling(), Times.Once);
        }

        RabbitTestHelp.DeleteExchange(busOptions);
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

        var busOptions = new BusOptions(exchangeName: "MicroserviceHostTest03");
        using (var target = new MicroserviceHost(eventListeners, busOptions))
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