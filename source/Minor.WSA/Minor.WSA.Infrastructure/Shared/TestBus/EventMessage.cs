namespace Minor.WSA.Infrastructure.Shared.TestBus
{
    public class EventMessage
    {
        private long timestamp;
        private string routingKey;
        private string correlationId;
        private string eventType;
        private string jsonMessage;

        public EventMessage(long timestamp, string routingKey, string correlationId, string eventType, string jsonMessage)
        {
            this.timestamp = timestamp;
            this.routingKey = routingKey;
            this.correlationId = correlationId;
            this.eventType = eventType;
            this.jsonMessage = jsonMessage;
        }
    }
}