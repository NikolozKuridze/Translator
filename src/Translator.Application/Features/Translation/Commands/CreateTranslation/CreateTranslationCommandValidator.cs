using FluentValidation;
using Translator.Infrastructure.Database.Postgres.Configurations.Constants;

namespace Translator.Application.Features.Translation.Commands.CreateTranslation;

public class CreateTranslationCommandValidator : AbstractValidator<CreateTranslationCommand>
{
    public CreateTranslationCommandValidator()
    {
        RuleFor(command => command.ValueName)
            .NotEmpty()
            .WithMessage("Value cannot be empty")
            .MaximumLength(DatabaseConstants.Translation.VALUE_MAX_LENGTH)
            .WithMessage("Value cannot be longer than " + DatabaseConstants.Translation.VALUE_MAX_LENGTH);
    }   
}