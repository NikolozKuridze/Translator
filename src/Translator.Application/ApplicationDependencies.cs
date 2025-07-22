using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Translator.Application.Features.Template.Commands.CreateTemplate;

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