using DeepL;
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
using Translator.Infrastructure.External.DeepL;

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
        services.AddSingleton<DeepLClient>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var key = config["DeepL:ApiKey"];
            var options = new DeepLClientOptions
            {
                sendPlatformInfo = false,
                ServerUrl = "https://api-free.deepl.com" 
                // "https://api.deepl.com" for pro
            };
            return new DeepLClient(key!, options);
        });
        
        services.AddSingleton<IRedisService, RedisService>();
        services.AddSingleton<TemplateCacheService>();
        services.AddSingleton<ValueCacheService>();
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
        

        services.AddScoped<ITranslationService, DeepLTranslationService>();

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
    }

    private static void AddConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LanguageSeedingConfiguration>
            (configuration.GetSection(nameof(LanguageSeedingConfiguration)));
    }
}