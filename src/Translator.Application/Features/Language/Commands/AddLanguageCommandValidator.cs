using FluentValidation;
using Translator.Infrastructure.Database.Postgres.Constants;

namespace Translator.Application.Features.Language.Commands;

public class AddLanguageCommandValidator : AbstractValidator<AddLanguageCommand>
{
    public AddLanguageCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty()
            .WithMessage("Name cannot be empty.")
            .Length(2, LanguageConstants.NAME_MAX_LENGTH)
            .WithMessage($"Name cannot be longer than {LanguageConstants.NAME_MAX_LENGTH} characters.");

        RuleFor(c => c.Code)
            .NotEmpty()
            .WithMessage("Code cannot be empty.")
            .Length(1, LanguageConstants.CODE_MAX_LENGTH)
            .WithMessage($"Code cannot be longer than {LanguageConstants.CODE_MAX_LENGTH} characters.");
        RuleFor(c => c.UnicodeRange)
            .NotEmpty()
            .WithMessage("UnicodeRange cannot be empty.")
            .Matches("^([0-9A-F]{4,6}-[0-9A-F]{4,6})(;[0-9A-F]{4,6}-[0-9A-F]{4,6})*$")
            .WithErrorCode("UnicodeRange don't match to normal format");
    }
}