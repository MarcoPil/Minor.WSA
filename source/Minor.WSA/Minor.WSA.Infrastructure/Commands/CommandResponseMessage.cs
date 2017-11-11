namespace Minor.WSA.Infrastructure
{
    public class CommandResponseMessage
    {
        public string CallbackQueueName { get; }
        public string CorrelationId { get; }
        public string Type { get; }
        public string JsonMessage { get; }

        public CommandResponseMessage(string callbackQueueName, string correlationId, string type, string jsonMessage)
        {
            CorrelationId = correlationId;
            CallbackQueueName = callbackQueueName;
            Type = type;
            JsonMessage = jsonMessage;
        }
    }
}