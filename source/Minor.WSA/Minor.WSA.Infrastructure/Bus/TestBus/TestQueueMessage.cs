namespace Minor.WSA.Infrastructure.TestBus
{
    public class TestQueueMessage
    {
        public long? Timestamp { get; }
        public string RoutingKey { get; }
        public string ReplyTo { get; }
        public string CorrelationId { get; }
        public string Type { get; }
        public string JsonBody { get; }

        public TestQueueMessage(long? timestamp = null, string routingKey = null, string replyTo = null, string correlationId = null, string type = null, string jsonBody = null)
        {
            Timestamp = timestamp;
            RoutingKey = routingKey;
            ReplyTo = replyTo;
            CorrelationId = correlationId;
            Type = type;
            JsonBody = jsonBody;
        }
    }
}