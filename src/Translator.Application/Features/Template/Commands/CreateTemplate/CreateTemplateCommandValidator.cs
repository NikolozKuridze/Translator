using FluentValidation;
using Translator.Infrastructure.Database.Postgres.Configurations.Constants;

namespace Translator.Application.Features.Template.Commands.CreateTemplate;

public class CreateTemplateCommandValidator : AbstractValidator<CreateTemplateCommand>
{
    public CreateTemplateCommandValidator()
    {
        RuleFor(x => x.TemplateName)
            .NotEmpty()
            .WithMessage("Template name cannot be empty")
            .Length(3, DatabaseConstants.Template.TEMPLATE_NAME_MAX_LENGTH);

        RuleFor(x => x.Values)
            .NotEmpty()
            .WithMessage("Template cannot be empty");
    }
}