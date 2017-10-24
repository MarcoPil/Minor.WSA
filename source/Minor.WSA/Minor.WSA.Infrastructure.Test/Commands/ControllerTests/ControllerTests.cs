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

    [Fact]
    public void StartHandlingCallsStartHandlingOnProvider()
    {
        var queueName = "test3QueueName";
        var commandHandlers = new Dictionary<string, ICommandHandler>();

        var mock = new Mock<TestBusOptions>(MockBehavior.Loose);
        var providerMock = new Mock<IBusProvider>(MockBehavior.Strict);
        mock.Setup(option => option.Provider).Returns(providerMock.Object);
        providerMock.Setup(p => p.CreateQueue(It.IsAny<string>()));
        providerMock.Setup(p => p.StartReceivingCommands(queueName, It.IsAny<CommandReceivedCallback>()))
                    .Verifiable();

        var target = new Controller(queueName, commandHandlers);
        target.OpenCommandQueue(mock.Object);

        target.StartHandling();

        providerMock.VerifyAll();
    }
    #region Intialize Controller
    private static void InitializeController(Dictionary<string, ICommandHandler> commandHandlers, out CommandReceivedCallback commandReceivedCallback)
    {
        var queueName = "test4QueueName";
        CommandReceivedCallback result = null;

        var mock = new Mock<TestBusOptions>(MockBehavior.Loose);
        var providerMock = new Mock<IBusProvider>(MockBehavior.Strict);
        mock.Setup(option => option.Provider).Returns(providerMock.Object);
        providerMock.Setup(p => p.CreateQueue(It.IsAny<string>()));
        providerMock.Setup(p => p.StartReceivingCommands(queueName, It.IsAny<CommandReceivedCallback>()))
                    .Callback((string s, CommandReceivedCallback c) => { result = c; });

        var target = new Controller(queueName, commandHandlers);
        target.OpenCommandQueue(mock.Object);
        target.StartHandling();
        commandReceivedCallback = result;
    }
    #endregion

    [Fact]
    public void OnCommandReceivedCommandIsDispatched()
    {
        var crm = new CommandReceivedMessage("cQN", "cId", "Command1", "jsonMessage");

        var handler1Mock = new Mock<ICommandHandler>(MockBehavior.Strict);
        handler1Mock.Setup(ch => ch.DispatchCommand(crm)).Returns<CommandResultMessage>(null).Verifiable();
        var commandHandlers = new Dictionary<string, ICommandHandler>() {
            { "Command1", handler1Mock.Object }
        };
        InitializeController(commandHandlers, out CommandReceivedCallback commandReceivedCallback);

        Assert.NotNull(commandReceivedCallback);
        commandReceivedCallback.Invoke(crm);

        handler1Mock.VerifyAll();
    }

    [Fact]
    public void OnCommandReceivedCommandIsDispatchedToTheRightHandle()
    {
        var crm1 = new CommandReceivedMessage("cQN", "cId", "Command1", "jsonMessage");
        var handler1Mock = new Mock<ICommandHandler>(MockBehavior.Strict);
        handler1Mock.Setup(ch => ch.DispatchCommand(crm1)).Returns<CommandResultMessage>(null).Verifiable();

        var crm2 = new CommandReceivedMessage("cQN", "cId", "Command2", "jsonMessage");
        var handler2Mock = new Mock<ICommandHandler>(MockBehavior.Strict);
        handler2Mock.Setup(ch => ch.DispatchCommand(crm2)).Returns<CommandResultMessage>(null).Verifiable();

        var commandHandlers = new Dictionary<string, ICommandHandler>() {
            { "Command1", handler1Mock.Object },
            { "Command2", handler2Mock.Object },
        };
        InitializeController(commandHandlers, out CommandReceivedCallback commandReceivedCallback);

        commandReceivedCallback.Invoke(crm2);
        commandReceivedCallback.Invoke(crm1);

        handler1Mock.VerifyAll();
        handler2Mock.VerifyAll();
    }

    [Fact]
    public void OnCommandReceived_UnknownCommandIsNotDispatched()
    {
        var crm1 = new CommandReceivedMessage("cQN", "cId", "Command1", "jsonMessage");
        var handler1Mock = new Mock<ICommandHandler>(MockBehavior.Strict);
        handler1Mock.Setup(ch => ch.DispatchCommand(crm1));

        var commandHandlers = new Dictionary<string, ICommandHandler>() {
            { "Command1", handler1Mock.Object },
        };
        InitializeController(commandHandlers, out CommandReceivedCallback commandReceivedCallback);

        var unkown = new CommandReceivedMessage("cQN", "cId", "Command2", "jsonMessage");
        commandReceivedCallback.Invoke(unkown);

        handler1Mock.Verify(ch => ch.DispatchCommand(crm1), Times.Never);
    }

    [Fact]
    public void OnCommandReceived_ResultIsReturned()
    {
        var crm1 = new CommandReceivedMessage("cQN", "cId", "Command1", "jsonMessage");
        var resultMessage = new CommandResultMessage("ResultType", "resultJsonMessage");
        var handler1Mock = new Mock<ICommandHandler>(MockBehavior.Strict);
        handler1Mock.Setup(ch => ch.DispatchCommand(crm1))
                    .Returns(resultMessage);

        var commandHandlers = new Dictionary<string, ICommandHandler>() {
            { "Command1", handler1Mock.Object },
        };
        InitializeController(commandHandlers, out CommandReceivedCallback commandReceivedCallback);

        var result = commandReceivedCallback.Invoke(crm1);

        Assert.Equal(resultMessage, result);
    }

    [Fact]
    public void OnCommandReceived_UnknownCommandReturnsTechnicalException()
    {
        var commandHandlers = new Dictionary<string, ICommandHandler>();
        InitializeController(commandHandlers, out CommandReceivedCallback commandReceivedCallback);

        var unkown = new CommandReceivedMessage("cQN", "cId", "Command2", "jsonMessage");
        var result = commandReceivedCallback.Invoke(unkown);

        var errorJson = "{\"Code\":404,\"Message\":\"Cannot Execute 'Command2'. Command not found.\"}";
        Assert.Equal("TechnicalError", result.Type);
        Assert.Equal(errorJson, result.JsonMessage);
    }

    [Fact]
    public void IfDispatchedCommandThrowsExceptionResultMustBeInternalServerError()
    {
        var crm1 = new CommandReceivedMessage("cQN", "cId", "Command1", "jsonMessage");
        var handler1Mock = new Mock<ICommandHandler>(MockBehavior.Strict);
        handler1Mock.Setup(ch => ch.DispatchCommand(crm1))
                    .Throws(new DivideByZeroException());

        var commandHandlers = new Dictionary<string, ICommandHandler>() {
            { "Command1", handler1Mock.Object },
        };
        InitializeController(commandHandlers, out CommandReceivedCallback commandReceivedCallback);

        var result = commandReceivedCallback.Invoke(crm1);

        var errorJson = "{\"Code\":501,\"Message\":\"Internal Server Error\"}";
        Assert.Equal("TechnicalError", result.Type);
        Assert.Equal(errorJson, result.JsonMessage);
    }

    [Fact]
    public void IfDispatchedCommandThrowsFunctionExceptionthenTheExeptionIsPassedToClient()
    {

        var crm1 = new CommandReceivedMessage("cQN", "cId", "Command1", "jsonMessage");
        var fex = new FunctionalException();
        fex.Add(new Error("US201", "Name cannot be empty"));
        fex.Add(new Error("US203-a", "Never bring a sword to a gun fight"));

        var handler1Mock = new Mock<ICommandHandler>(MockBehavior.Strict);
        handler1Mock.Setup(ch => ch.DispatchCommand(crm1))
                    .Throws(fex);

        var commandHandlers = new Dictionary<string, ICommandHandler>() {
            { "Command1", handler1Mock.Object },
        };
        InitializeController(commandHandlers, out CommandReceivedCallback commandReceivedCallback);

        var result = commandReceivedCallback.Invoke(crm1);

        var errorJson = "[{\"Code\":\"US201\",\"Message\":\"Name cannot be empty\"},{\"Code\":\"US203-a\",\"Message\":\"Never bring a sword to a gun fight\"}]";
        Assert.Equal("FunctionalException", result.Type);
        Assert.Equal(errorJson, result.JsonMessage);
    }
}