using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Translator.Application.Features.Translation.Commands;

namespace Translator.Application;

public static class ApplicationDependencies
{
    public static void AddApplicationDependencies(this IServiceCollection services)
    {
        services.AddMediatR(cfg => { cfg.RegisterServicesFromAssemblies(typeof(ApplicationDependencies).Assembly); });

        services.AddValidatorsFromAssemblyContaining<CreateTranslation.Validator>(includeInternalTypes: true);
    }
}