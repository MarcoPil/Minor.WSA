using Minor.WSA.Infrastructure;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

public class CommandHandlerTests
{
    [Fact]
    public void CommandHandlerExecutesCommand()
    {
        var testController = new DummyController();
        var factoryMock = new Mock<IFactory>();
        factoryMock.Setup(fm => fm.GetInstance()).Returns(testController);

        var factory = factoryMock.Object;
        var method = typeof(DummyController).GetMethod("SayHello");
        var returnType = typeof(string);
        var paramType = typeof(string);
        var target = new CommandHandler(factory, method, returnType, paramType);

        var receivedMessage = new CommandReceivedMessage("cQN","cId","System.String","\"World\"");
        var resultMessage = target.DispatchCommand(receivedMessage);

        Assert.Equal("System.String", resultMessage.Type);
        Assert.Equal("\"Hello, World\"", resultMessage.JsonMessage);
    }
    #region DummyController
    private class DummyController
    {
        public string SayHello(string name)
        {
            return "Hello, " + name;
        }
    }
    #endregion
}
