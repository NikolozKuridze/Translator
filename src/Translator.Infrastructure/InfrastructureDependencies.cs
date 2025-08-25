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
        AddRedisOptions(services, configuration);
        AddSingletonDependencies(services);
        AddConfigurations(services, configuration);
    }

    private static void AddRedisOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RedisOptions>(
            configuration.GetSection(nameof(RedisOptions)));

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisOptions = sp.GetRequiredService<IOptions<RedisOptions>>().Value;
            var configOptions = new ConfigurationOptions
            {
                EndPoints = { redisOptions.ConnectionString },
                Password = redisOptions.Password,
                AbortOnConnectFail = false,
                ConnectTimeout = 5000,
                SyncTimeout = 5000,
            };

            return ConnectionMultiplexer.Connect(configOptions);
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