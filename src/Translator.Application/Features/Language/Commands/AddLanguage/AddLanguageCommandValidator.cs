using FluentValidation;
using Translator.Infrastructure.Database.Postgres.Configurations.Constants;

namespace Translator.Application.Features.Language.Commands.AddLanguage;

/*public class AddLanguageCommandValidator : AbstractValidator<AddLanguageCommand>
{
    public AddLanguageCommandValidator()
    {
        RuleFor(c => c.Code)
            .NotEmpty()
            .WithMessage("Code cannot be empty.")
            .Length(1, DatabaseConstants.Language.CODE_MAX_LENGTH)
            .WithMessage($"Code cannot be longer than {DatabaseConstants.Language.CODE_MAX_LENGTH} characters.");
    }
}*/