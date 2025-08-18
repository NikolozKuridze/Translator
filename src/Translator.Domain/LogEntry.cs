    namespace Translator.Domain;

    public class LogEntry
    {
        public string Message { get; set; } = "";
        public string MessageTemplate { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public int Level { get; set; }
        public string? Exception { get; set; }
        public string? LogEvent { get; set; }
    }