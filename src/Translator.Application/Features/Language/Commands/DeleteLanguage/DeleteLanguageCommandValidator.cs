using FluentValidation;
using Translator.Infrastructure.Database.Postgres.Configurations.Constants;

namespace Translator.Application.Features.Language.Commands.DeleteLanguage;

public class DeleteLanguageCommandValidator : AbstractValidator<DeleteLanguageCommand>
{
    public DeleteLanguageCommandValidator()
    {
        RuleFor(c => c.Code)
            .NotEmpty()
            .WithMessage("Code is required.")
            .Length(2, DatabaseConstants.Language.CODE_MAX_LENGTH)
            .WithMessage($"Code length must be between 2 and {DatabaseConstants.Language.CODE_MAX_LENGTH}");
    }
}