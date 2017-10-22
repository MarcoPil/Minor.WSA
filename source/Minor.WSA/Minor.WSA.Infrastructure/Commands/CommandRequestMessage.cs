namespace Minor.WSA.Infrastructure
{
    public class CommandRequestMessage
    {
        public string ServiceQueueName { get; }
        public string CommandType { get; }
        public string JsonMessage { get; }

        public CommandRequestMessage(string serviceQueueName, string commandType, string jsonMessage)
        {
            ServiceQueueName = serviceQueueName;
            CommandType = commandType;
            JsonMessage = jsonMessage;
        }
    }
}