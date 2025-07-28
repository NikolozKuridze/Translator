namespace Translator.Infrastructure.Database.Redis;

public class RedisConfiguration
{
    public string ConnectionString { get; set; } = null!;
    public int DefaultCacheExpirationMinutes { get; set; }
}
