using Minor.WSA.Infrastructure;
using Minor.WSA.Infrastructure.Test.EventHandlerTests;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

public class EventDispatcherTest
{
    [Fact]
    public void DispatchesEvent()
    {
        // Arrange
        var testHandler = new DispatcherTestMock();
        var factoryMock = new Mock<IFactory>();
        factoryMock.Setup(fm => fm.GetInstance()).Returns(testHandler);

        var factory = factoryMock.Object;
        var method = typeof(DispatcherTestMock).GetMethod("HandleDispatchTestEvent");
        var paramType = typeof(DispatchTestEvent);

        var target = new EventDispatcher(factory, method, paramType);

        // Act
        string jsonMessage = "{\"Number\":7,\"RoutingKey\":\"MVM.Test.DispatchTest\",\"Timestamp\":636209314900846110,\"ID\":\"75236abd-078e-4855-a83b-a9cb5d61a47a\"}";
        target.DispatchEvent(jsonMessage);

        // Assert
        Assert.NotNull(testHandler.EventReceived);
        Assert.Equal(7, testHandler.EventReceived.Number);
        Assert.Equal("MVM.Test.DispatchTest", testHandler.EventReceived.RoutingKey);
        Assert.Equal(636209314900846110, testHandler.EventReceived.Timestamp);
        Assert.Equal("75236abd-078e-4855-a83b-a9cb5d61a47a", testHandler.EventReceived.ID.ToString());
    }

    [Fact]
    public void DispatchesEvent_JsonHasMorePrepertiesThanLocalEvent()
    {
        // Arrange
        var testHandler = new DispatcherTestMock();
        var factoryMock = new Mock<IFactory>();
        factoryMock.Setup(fm => fm.GetInstance()).Returns(testHandler);

        var factory = factoryMock.Object;
        var method = typeof(DispatcherTestMock).GetMethod("HandleDispatchTestEvent");
        var paramType = typeof(DispatchTestEvent);

        var target = new EventDispatcher(factory, method, paramType);

        // Act
        string jsonMessage = "{\"Thing\":16,\"Number\":17,\"otherThing\":18,\"RoutingKey\":\"MVM.Test.DispatchTest\",\"Timestamp\":636209314900846111,\"ID\":\"75236abd-078e-4855-a83b-a9cb5d61a47b\"}";
        target.DispatchEvent(jsonMessage);

        // Assert
        Assert.NotNull(testHandler.EventReceived);
        Assert.Equal(17, testHandler.EventReceived.Number);
        Assert.Equal("MVM.Test.DispatchTest", testHandler.EventReceived.RoutingKey);
        Assert.Equal(636209314900846111, testHandler.EventReceived.Timestamp);
        Assert.Equal("75236abd-078e-4855-a83b-a9cb5d61a47b", testHandler.EventReceived.ID.ToString());
    }


    [Fact]
    public void DispatchesEvent_JsonHasFewerPrepertiesThanLocalEvent()
    {
        // Arrange
        var testHandler = new DispatcherTestMock();
        var factoryMock = new Mock<IFactory>();
        factoryMock.Setup(fm => fm.GetInstance()).Returns(testHandler);

        var factory = factoryMock.Object;
        var method = typeof(DispatcherTestMock).GetMethod("HandleDispatchTestEvent");
        var paramType = typeof(DispatchTestEvent);

        var target = new EventDispatcher(factory, method, paramType);

        // Act
        string jsonMessage = "{\"RoutingKey\":\"MVM.Test.DispatchTest\",\"Timestamp\":636209314900846112,\"ID\":\"75236abd-078e-4855-a83b-a9cb5d61a47c\"}";
        target.DispatchEvent(jsonMessage);

        // Assert
        Assert.NotNull(testHandler.EventReceived);
        Assert.Equal(0, testHandler.EventReceived.Number);  // default value
        Assert.Equal("MVM.Test.DispatchTest", testHandler.EventReceived.RoutingKey);
        Assert.Equal(636209314900846112, testHandler.EventReceived.Timestamp);
        Assert.Equal("75236abd-078e-4855-a83b-a9cb5d61a47c", testHandler.EventReceived.ID.ToString());
    }

}