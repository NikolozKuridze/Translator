using FluentValidation;
using Translator.Infrastructure.Database.Postgres.Constants;

namespace Translator.Application.Features.Language.Commands.AddLanguage;

public class AddLanguageCommandValidator : AbstractValidator<AddLanguageCommand>
{
    public AddLanguageCommandValidator()
    {
        RuleFor(c => c.Code)
            .NotEmpty()
            .WithMessage("Code cannot be empty.")
            .Length(1, LanguageConstants.CODE_MAX_LENGTH)
            .WithMessage($"Code cannot be longer than {LanguageConstants.CODE_MAX_LENGTH} characters.");
    }
}