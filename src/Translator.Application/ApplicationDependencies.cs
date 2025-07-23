using Microsoft.Extensions.DependencyInjection;

namespace Translator.Application;

public static class ApplicationDependencies
{
    public static void AddApplicationDependencies(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(typeof(ApplicationDependencies).Assembly);
        });
    }
}