using Minor.WSA.AuditLog.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Minor.WSA.AuditLog.DAL
{
    public class LogRepository : ILogRepository
    {
        private readonly LoggerContext _context;

        public LogRepository(LoggerContext context)
        {
            _context = context;
        }

        public void AddEntry(LogEntry entry)
        {
            _context.LogEntries.Add(entry);
            _context.SaveChanges();
        }

        public IEnumerable<LogEntry> FindEntriesBy(LogEntryCriteria criteria)
        {

            IQueryable<LogEntry> result = _context.LogEntries;

            result = result.Where(entry =>
                (criteria.FromTimestamp == null || entry.Timestamp >= criteria.FromTimestamp) &&
                (criteria.ToTimestamp == null || entry.Timestamp <= criteria.ToTimestamp) &&
                (criteria.EventType == null || entry.EventType == criteria.EventType)
            );

            if (criteria.Topic != null)
            {
                var pattern = criteria.Topic
                                      .Replace(@".", @"\.")
                                      .Replace(@"*", @"[^.]*")
                                      .Replace(@"#", @".*");
                pattern = "^" + pattern + "$";
                Regex regex = new Regex(pattern);

                result = result.Where(entry => regex.IsMatch(entry.RoutingKey));
            }
            return result.ToList();
        }
    }
}