using FluentValidation;
using Translator.Infrastructure.Database.Postgres.Constants;

namespace Translator.Application.Features.Language.DeleteLanguage;

public class DeleteLanguageCommandValidator : AbstractValidator<DeleteLanguageCommand>
{
    public DeleteLanguageCommandValidator()
    {
        RuleFor(c => c.Code)
            .NotEmpty()
            .WithMessage("Code is required.")
            .Length(2, LanguageConstants.CODE_MAX_LENGTH)
            .WithMessage($"Code length must be between 2 and {LanguageConstants.CODE_MAX_LENGTH}");
    }
}