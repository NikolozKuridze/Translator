using Google.Cloud.Translation.V2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Translator.Infrastructure.Configurations;
using Translator.Infrastructure.Database.Postgres;
using Translator.Infrastructure.Database.Postgres.Repository;
using Translator.Infrastructure.Database.Redis;
using Translator.Infrastructure.Database.Redis.CacheServices;
using Translator.Infrastructure.GoogleService;

namespace Translator.Infrastructure;

public static class InfrastructureDependencies 
{

    public static void AddInfrastructureDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        // postgres
        services
            .AddDbContext<ApplicationDbContext>(cfg =>
            {
                cfg
                    .UseNpgsql(configuration.GetConnectionString(nameof(Postgres)))
                    .UseSnakeCaseNamingConvention();
            });

        services
            .AddDbContext<LogsDbContext>(cfg =>
            {
                cfg.UseNpgsql(configuration.GetConnectionString("LogsDb"));
            });
        
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        
        
        // redis
        services.Configure<RedisConfiguration>(
           configuration.GetSection(nameof(RedisConfiguration)));

        services.AddSingleton<IRedisService, RedisService>();

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisConfig = sp.GetRequiredService<IOptions<RedisConfiguration>>().Value;
            return ConnectionMultiplexer.Connect(redisConfig.ConnectionString);
        });

        services.AddSingleton<TemplateCacheService>();
        services.AddSingleton<ValueCacheService>();
        
        // google translate
        services.AddSingleton<TranslationClient>(sp
            => TranslationClient.Create());
        services.AddSingleton<ITranslationService, GoogleTranslationService>();


    }
}