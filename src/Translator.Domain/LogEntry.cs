namespace Translator.Domain;

public class LogEntry
{
    public long Id { get; set; }
    public string Message { get; private set; } = string.Empty;
    public string MessageTemplate { get; private set; } = string.Empty;
    public DateTime Timestamp { get; private set; }
    public int Level { get; private set; }
    public string? Exception { get; private set; }
    public string? LogEvent { get; private set; }
}