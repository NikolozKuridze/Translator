using FluentValidation;
using Translator.Infrastructure.Database.Postgres.Constants;

namespace Translator.Application.Features.Translation.Commands.CreateTranslation;

public class CreateTranslationCommandValidator : AbstractValidator<CreateTranslationCommand>
{
    public CreateTranslationCommandValidator()
    {
        RuleFor(command => command.Value)
            .NotEmpty()
            .WithMessage("Value cannot be empty")
            .MaximumLength(TranslationConstants.VALUE_MAX_LENGTH)
            .WithMessage("Value cannot be longer than " + TranslationConstants.VALUE_MAX_LENGTH);
    }   
}