namespace Minor.WSA.Infrastructure
{
    public class CommandReceivedMessage
    {
        public string CallbackQueueName { get; }
        public string CorrelationId { get; }
        public string CommandType { get; }
        public string JsonMessage { get; }

        public CommandReceivedMessage(string callbackQueueName, string correlationId, string commandType, string jsonMessage)
        {
            CallbackQueueName = callbackQueueName;
            CorrelationId = correlationId;
            CommandType = commandType;
            JsonMessage = jsonMessage;
        }
    }
}