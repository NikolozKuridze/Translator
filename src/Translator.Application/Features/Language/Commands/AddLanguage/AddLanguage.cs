using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Infrastructure.Database.Postgres.Configurations.Constants;
using Translator.Infrastructure.Database.Postgres.Repository;
using LanguageEntity = Translator.Domain.Entities.Language;

namespace Translator.Application.Features.Language.Commands.AddLanguage;

public abstract class AddLanguage
{
    public record AddLanguageCommand(string Code) : IRequest<AddLanguageResponse>;

    public record AddLanguageResponse(string LanguageCode);

    public class Validator : AbstractValidator<AddLanguageCommand>
    {
        public Validator()
        {
            RuleFor(c => c.Code)
                .NotEmpty()
                .WithMessage("Code cannot be empty.")
                .Length(1, DatabaseConstants.Language.CODE_MAX_LENGTH)
                .WithMessage($"Code cannot be longer than {DatabaseConstants.Language.CODE_MAX_LENGTH} characters.");
        }
    }

    public class Handler(IRepository<LanguageEntity> repository, IValidator<AddLanguageCommand> validator)
        : IRequestHandler<AddLanguageCommand, AddLanguageResponse>
    {
        public async Task<AddLanguageResponse> Handle(AddLanguageCommand request, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            var existsLanguage = await repository
                .Where(x =>
                    x.Code == request.Code
                    && x.IsActive == false)
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(cancellationToken);

            if (existsLanguage is null)
                throw new LanguageAlreadyAdded(request.Code);

            existsLanguage.IsActive = true;
            await repository.SaveChangesAsync(cancellationToken);

            return new AddLanguageResponse(request.Code);
        }
    }
}