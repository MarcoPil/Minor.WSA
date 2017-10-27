using System;
using System.Collections.Generic;

namespace Minor.WSA.Infrastructure.Shared.TestBus
{
    public class TestCommandQueue
    {
        public string QueueName { get; }
        public Queue<CommandRequestMessage> CommandQueue { get; }
        public CommandReceivedCallback CommandReceived;

        public TestCommandQueue(string queueName)
        {
            QueueName = queueName;
            CommandQueue = new Queue<CommandRequestMessage>();
        }

        public void SendCommandAsync(CommandRequestMessage command)
        {
            if (CommandReceived == null)
            {
                CommandQueue.Enqueue(command);
            }
            else
            {
                ProcessCommandRequestMessage(command);
            }
        }

        private CommandResultMessage ProcessCommandRequestMessage(CommandRequestMessage command)
        {
            var received = new CommandReceivedMessage(
                callbackQueueName: "Q",
                correlationId: Guid.NewGuid().ToString(),
                commandType: command.CommandType,
                jsonMessage: command.JsonMessage);
            return CommandReceived.Invoke(received);
        }

        public void Register(CommandReceivedCallback callback)
        {
            CommandReceived += callback;
            while (CommandQueue.Count > 0)
            {
                CommandRequestMessage command = CommandQueue.Dequeue();
                ProcessCommandRequestMessage(command);
            }
        }
    }
}