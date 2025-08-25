namespace Translator.Infrastructure.Database.Redis.Rudiment;

public class RedisOptions
{
    public string ConnectionString { get; set; } = null!;
    public int SessionTimeout { get; set; }
    public string? Password { get; set; }
    public int DefaultCacheExpirationDays { get; set; }
}