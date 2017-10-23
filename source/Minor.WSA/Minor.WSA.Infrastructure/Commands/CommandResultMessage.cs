namespace Minor.WSA.Infrastructure
{
    public class CommandResultMessage
    {
        public string Type { get; }
        public string JsonMessage { get; }

        public CommandResultMessage(string type, string jsonMessage)
        {
            Type = type;
            JsonMessage = jsonMessage;
        }
    }
}