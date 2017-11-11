using Minor.WSA.AuditLog.Entities;
using System.Collections.Generic;

namespace Minor.WSA.AuditLog.DAL
{
    public interface ILogRepository
    {
        void AddEntry(LogEntry entry);
        IEnumerable<LogEntry> FindEntriesBy(LogEntryCriteria criteria);
    }
}