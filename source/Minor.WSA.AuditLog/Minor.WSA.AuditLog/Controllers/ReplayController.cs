using Minor.WSA.AuditLog.Commands;
using Minor.WSA.AuditLog.DAL;
using Minor.WSA.AuditLog.Entities;
using Minor.WSA.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.WSA.AuditLog.Controllers
{
    [Controller("AuditlogReplayService")]
    public class ReplayController
    {
        private readonly ILogRepository _logRepo;
        private readonly BusOptions _busOptions;

        public ReplayController(ILogRepository logRepo, BusOptions busOptions)
        {
            _logRepo = logRepo;
            _busOptions = busOptions;
        }

        [Execute]
        public void ReplayEvents(ReplayEventsCommand replayEventsCommand)
        {
            // Get all events that match the search criteria
            LogEntryCriteria replaycriteria = LogEntryCriteriaFromCommand(replayEventsCommand);
            var replayEntries = _logRepo.FindEntriesBy(replaycriteria);

            // Publish all events on the replay-exchange
            var replayBusOptions = _busOptions.CopyWith(exchangeName: replayEventsCommand.ExchangeName);
            foreach (var logEntry in replayEntries)
            {
                var eventMessage = EventMessageFromLogEntry(logEntry);
                replayBusOptions.Provider.PublishEvent(eventMessage);
            }
        }


        private static LogEntryCriteria LogEntryCriteriaFromCommand(ReplayEventsCommand replayEventsCommand)
        {
            return new LogEntryCriteria
            {
                FromTimestamp = replayEventsCommand.FromTimestamp,
                ToTimestamp = replayEventsCommand.ToTimestamp,
                EventType = replayEventsCommand.EventType,
                Topic = replayEventsCommand.Topic,
            };
        }

        private static EventMessage EventMessageFromLogEntry(LogEntry entry)
        {
            return new EventMessage(
                timestamp: entry.Timestamp,
                routingKey: entry.RoutingKey,
                correlationId: entry.CorrelationId,
                eventType: entry.EventType,
                jsonMessage: entry.JsonMessage
            );
        }
    }
}
