using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Minor.WSA.Infrastructure;
using Minor.WSA.AuditLog.Entities;

namespace Minor.WSA.AuditLog.DAL
{
    public class LoggerContext : DbContext
    {
        public LoggerContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<LogEntry> LogEntries { get; set; }
    }
}
