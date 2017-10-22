using Minor.WSA.Infrastructure;
using Minor.WSA.Infrastructure.Shared.TestBus;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

public class ControllerTests
{
    [Fact]
    public void OpenCommandQueueCallsCreateQueueOnProvider()
    {
        var queueName = "test1QueueName";
        var commandHandlers = new Dictionary<string, ICommandHandler>();

        var mock = new Mock<TestBusOptions>(MockBehavior.Loose);
        var providerMock = new Mock<IBusProvider>(MockBehavior.Strict);
        mock.Setup(option => option.Provider).Returns(providerMock.Object);
        providerMock.Setup(p => p.CreateQueue(queueName)).Verifiable();

        var target = new Controller(queueName, commandHandlers);

        target.OpenCommandQueue(mock.Object);

        providerMock.VerifyAll();
    }

    [Fact]
    public void OpenCommandQueueSetsBusOptions()
    {
        var mock = new Mock<TestBusOptions>(MockBehavior.Loose);
        var providerMock = new Mock<IBusProvider>(MockBehavior.Loose);
        mock.Setup(option => option.Provider).Returns(providerMock.Object);
        var busOptions = mock.Object;
        var target = new Controller("test2QueueName", null);

        target.OpenCommandQueue(busOptions);

        Assert.Equal(busOptions, target.BusOptions);
    }
}