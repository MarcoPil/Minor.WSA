namespace Minor.WSA.AuditLog.Entities
{
    public class LogEntry
    {
        public long Id { get; set; }
        public long Timestamp { get; set; }
        public string CorrelationId { get; set; }
        public string RoutingKey { get; set; }
        public string EventType { get; set; }
        public string JsonMessage { get; set; }
    }
}