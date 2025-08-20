namespace Translator.Infrastructure.Database.Redis.Rudiment;

public class RedisConfiguration
{
    public string ConnectionString { get; set; } = null!;
    public int DefaultCacheExpirationDays { get; set; }
}
