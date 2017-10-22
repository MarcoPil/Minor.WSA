namespace Minor.WSA.Infrastructure
{
    public class CommandResultMessage
    {
        public string JsonMessage { get; }

        public CommandResultMessage(string jsonMessage)
        {
            JsonMessage = jsonMessage;
        }
    }
}