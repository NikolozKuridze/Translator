using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Translator.Infrastructure.Configurations;
using Translator.Infrastructure.Database.Postgres;
using Translator.Infrastructure.Database.Postgres.Repository;

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
        
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

    }
}