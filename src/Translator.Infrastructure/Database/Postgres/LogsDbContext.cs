using Microsoft.EntityFrameworkCore;
using Translator.Domain;

namespace Translator.Infrastructure.Database.Postgres;

public class LogsDbContext : DbContext
{
    public LogsDbContext(DbContextOptions<LogsDbContext> options) : base(options) { }

    public DbSet<LogEntry> Logs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LogEntry>()
            .HasNoKey();
        modelBuilder.Entity<LogEntry>()
            .ToTable("logs");
        
        modelBuilder.Entity<LogEntry>().Property(l => l.Timestamp).HasColumnName("timestamp");
        modelBuilder.Entity<LogEntry>().Property(l => l.Level).HasColumnName("level");
        modelBuilder.Entity<LogEntry>().Property(l => l.MessageTemplate).HasColumnName("message_template");
        modelBuilder.Entity<LogEntry>().Property(l => l.Message).HasColumnName("message");
        modelBuilder.Entity<LogEntry>().Property(l => l.Exception).HasColumnName("exception");
        modelBuilder.Entity<LogEntry>().Property(l => l.LogEvent).HasColumnName("log_event");
    }
}