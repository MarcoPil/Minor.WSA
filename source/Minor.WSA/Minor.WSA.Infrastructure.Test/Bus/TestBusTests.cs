using Minor.WSA.Infrastructure;
using Minor.WSA.Infrastructure.Shared.TestBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Xunit;

public class TestBusTests
{
    [Fact]
    public void CreatCommandQueueCreateCommandQueue()
    {
        var target = new TestBusProvider();

        target.CreateCommandQueue("text1CommandQ");

        Assert.Contains(target.Queues, q => q.QueueName == "text1CommandQ");
    }

    [Fact]
    public void CommandRequestsAreLogged()
    {
        var target = new TestBusProvider();
        target.CreateCommandQueue("text2CommandQ");

        var commandRequestMessage = new CommandRequestMessage("text2CommandQ", "CommandType", "payload");
        target.SendCommandAsync(commandRequestMessage);

        Assert.Contains(commandRequestMessage, target.LoggedCommandRequestMessages);
    }

    [Fact]
    public void CommandRequestsAreAddedToTheServiceQueue()
    {
        var target = new TestBusProvider();
        target.CreateCommandQueue("text3CommandQ");

        var commandRequestMessage = new CommandRequestMessage("text3CommandQ", "CommandType", "payload");
        target.SendCommandAsync(commandRequestMessage);

        var serviceQueue = target.Queues.Single(cq => cq.QueueName == "text3CommandQ");
        Assert.Contains(serviceQueue.Messages, m => m.JsonBody == "payload");
    }

    [Fact]
    public void CommandRequestsAreAddedToTheCorrectServiceQueue()
    {
        var target = new TestBusProvider();
        target.CreateCommandQueue("text4aCommandQ");
        target.CreateCommandQueue("text4bCommandQ");
        target.CreateCommandQueue("text4cCommandQ");

        var commandRequestMessage = new CommandRequestMessage("text4bCommandQ", "CommandType", "payload");
        target.SendCommandAsync(commandRequestMessage);

        var serviceQueue = target.Queues.Single(cq => cq.QueueName == "text4bCommandQ");
        Assert.Contains(serviceQueue.Messages, m => m.JsonBody == "payload");
    }

    [Fact]
    public void StartReceivingCommands_RegistersACallbackToThecommandQeueue()
    {
        var target = new TestBusProvider();
        target.CreateCommandQueue("text5CommandQ");

        CommandReceivedCallback callback = (crm) => null;
        target.StartReceivingCommands("text5CommandQ", callback);

        Assert.Contains(target.Queues, cq => cq.Callbacks != null);
    }

    [Fact]
    public void StartReceivingCommands_ThrowsExceptionIfQueueDoesNotExist()
    {
        var target = new TestBusProvider();
        target.CreateCommandQueue("text6CommandQ");

        CommandReceivedCallback callback = (crm) => null;
        Action action = () => target.StartReceivingCommands("non-existing name", callback);

        var ex = Assert.Throws<MicroserviceException>(action);
        Assert.Equal($"Cannot .StartReceivingCommands() on a non-existing queue. Queue with name 'non-existing name' does not exist. Consider calling .CreateCommandQueue(\"non-existing name\") first.", ex.Message);
    }

    [Fact]
    public void AfterSendingACommand_CorrectCallbackIsCalled()
    {
        var target = new TestBusProvider();
        target.CreateCommandQueue("text7CommandQ");

        CommandReceivedMessage receivedCommand = null;
        var handle = new AutoResetEvent(false);

        CommandReceivedCallback callback = (CommandReceivedMessage crm) =>
        {
            receivedCommand = crm;
            handle.Set();
            return null;
        };
        target.StartReceivingCommands("text7CommandQ", callback);
        var commandRequestMessage = new CommandRequestMessage("text7CommandQ", "CommandType", "payload");

        // Act
        target.SendCommandAsync(commandRequestMessage);

        bool received = handle.WaitOne(100);
        Assert.True(received);
//        Assert.NotNull(receivedCommand.CallbackQueueName);
//        Assert.NotNull(receivedCommand.CorrelationId);
        Assert.Equal("CommandType", receivedCommand.CommandType);
        Assert.Equal("payload", receivedCommand.JsonMessage);
    }


    [Fact]
    public void SentCommandsareProcessedWhenACallbackIsRegistered()
    {
        var target = new TestBusProvider();
        target.CreateCommandQueue("text8CommandQ");

        var commandRequestMessage = new CommandRequestMessage("text8CommandQ", "CommandType", "payload");
        target.SendCommandAsync(commandRequestMessage);
        target.SendCommandAsync(commandRequestMessage);
        target.SendCommandAsync(commandRequestMessage);


        int receiveCount = 0;
        var handle = new AutoResetEvent(false);

        CommandReceivedCallback callback = (CommandReceivedMessage crm) =>
        {
            receiveCount++;
            handle.Set();
            return null;
        };

        // Act
        target.StartReceivingCommands("text8CommandQ", callback);

        bool received = handle.WaitOne(100);
        Assert.True(received);
        Assert.Equal(3, receiveCount);
    }
}