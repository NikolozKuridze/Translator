using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Translator.Application.Features.Translation.Commands.CreateTranslation;

namespace Translator.Application;

public static class ApplicationDependencies
{
    public static void AddApplicationDependencies(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(typeof(ApplicationDependencies).Assembly);
        });

        services.AddValidatorsFromAssemblyContaining<CreateTranslationCommandValidator>(includeInternalTypes: true);
    }
}