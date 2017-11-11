using Minor.WSA.AuditLog.DAL;
using Minor.WSA.AuditLog.Entities;
using Minor.WSA.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.WSA.AuditLog.Listeners
{
    [EventListener("AuditLogListenQueue")]
    public class AllEventsListener
    {
        private readonly LoggerContext _context;

        [Topic("#")]
        public void ReveiveAllEvents(EventMessage eventMessage)
        {
            var logEntry = new LogEntry
            {
                Timestamp = eventMessage.Timestamp,
                CorrelationId = eventMessage.CorrelationId,
                RoutingKey = eventMessage.RoutingKey,
                EventType = eventMessage.EventType,
                JsonMessage = eventMessage.JsonMessage,
            };
            _context.LogEntries.Add(logEntry);
            _context.SaveChanges();
        }
    }
}
