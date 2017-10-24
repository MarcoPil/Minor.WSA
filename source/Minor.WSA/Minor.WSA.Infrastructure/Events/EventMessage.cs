namespace Minor.WSA.Infrastructure
{
    public class EventMessage
    {
        public long Timestamp { get; }
        public string RoutingKey { get; }
        public string CorrelationId { get; }
        public string EventType { get; }
        public string JsonMessage { get; }

        public EventMessage(long timestamp, string routingKey, string correlationId, string eventType, string jsonMessage)
        {
            Timestamp = timestamp;
            RoutingKey = routingKey;
            CorrelationId = correlationId;
            EventType = eventType;
            JsonMessage = jsonMessage;
        }
    }
}