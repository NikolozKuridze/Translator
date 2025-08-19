using Google.Cloud.Translation.V2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Translator.Domain;
using Translator.Infrastructure.Configurations;
using Translator.Infrastructure.Database.Postgres;
using Translator.Infrastructure.Database.Postgres.Repository;
using Translator.Infrastructure.Database.Redis.CacheServices;
using Translator.Infrastructure.Database.Redis.Rudiment;

namespace Translator.Infrastructure;

public static class InfrastructureDependencies 
{
    public static void AddInfrastructureDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        AddScopedServices(services, configuration);
        AddRedisConfiguration(services, configuration);
        AddSingletonDependencies(services);
        AddConfigurations(services, configuration);
    }

    private static void AddRedisConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RedisConfiguration>(
            configuration.GetSection(nameof(RedisConfiguration)));
        
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisConfig = sp.GetRequiredService<IOptions<RedisConfiguration>>().Value;
            return ConnectionMultiplexer.Connect(redisConfig.ConnectionString);
        });
    }

    private static void AddSingletonDependencies(this IServiceCollection services)
    {
        services.AddSingleton<IRedisService, RedisService>();
        services.AddSingleton<TemplateCacheService>();
        services.AddSingleton<ValueCacheService>();
        services.AddSingleton<TranslationClient>(sp
            => TranslationClient.Create()); 
    }

    private static void AddScopedServices(this IServiceCollection services, IConfiguration configuration)
    {
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
                cfg.UseNpgsql(configuration.GetConnectionString(nameof(LogEntry)));
            });
        
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
    }

    private static void AddConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LanguageSeedingConfiguration>
            (configuration.GetSection(nameof(LanguageSeedingConfiguration)));
    }
}