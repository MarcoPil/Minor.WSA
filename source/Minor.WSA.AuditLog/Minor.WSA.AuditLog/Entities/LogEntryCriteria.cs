namespace Minor.WSA.AuditLog.Entities
{
    public class LogEntryCriteria
    {
        public long? FromTimestamp { get; set; }
        public long? ToTimestamp { get; set; }
        public string EventType { get; set; }
        public string Topic { get; set; }
    }
}