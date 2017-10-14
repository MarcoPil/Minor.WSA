using Minor.WSA.Infrastructure;
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
            // Act
            target.StartHandling();

            // Assert
            mock1.Verify(el => el.StartProcessing(), Times.Once);
            mock2.Verify(el => el.StartProcessing(), Times.Once);
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
    //[Fact]
    //public void DispatchesEvents()
    //{
    //    var dispatcher = new EventDispatcher()
    //    var eventListeners = new List<EventListener>
    //    {
    //        new EventListener("MicroserviceHostTest_Q01", null),
    //    };

    //    var busOptions = new BusOptions(exchangeName: "MicroserviceHostTest01");
    //    using (var target = new MicroserviceHost(eventListeners, busOptions))
    //    {
    //        // Act
    //        target.OpenConnection();

    //        // Assert
    //        using (var publisher = new EventPublisher(busOptions))
    //        {
    //            publisher
    //        }
    //    }
    //    RabbitTestHelp.DeleteExchange(busOptions);
    //}





}