using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Configurations.Constants;
using Translator.Infrastructure.Database.Postgres.Repository;

namespace Translator.Application.Features.CategoryTypes.Commands;

public abstract class DeleteCategoryTypes
{
    public sealed record Command(
        IEnumerable<string> TypeNames) : IRequest;


    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.TypeNames)
                .Must(typeNames => typeNames.Any(name => !string.IsNullOrWhiteSpace(name)))
                .WithMessage("At least one non-empty type name is required.");

            RuleForEach(x => x.TypeNames)
                .NotEmpty()
                .WithMessage("Type name is required.")
                .MinimumLength(DatabaseConstants.Type.NAME_MIN_LENGTH)
                .WithMessage($"Type must be at least {DatabaseConstants.Type.NAME_MIN_LENGTH} characters.")
                .MaximumLength(DatabaseConstants.Type.NAME_MAX_LENGTH)
                .WithMessage($"Type cannot be longer than {DatabaseConstants.Type.NAME_MAX_LENGTH} characters.")
                .Matches(@"^[a-zA-Z-]+$")
                .WithMessage("Type name can only contain letters and hyphens.");
        }
    }

    public class Handler(IRepository<CategoryType> typeRepository, IValidator<Command> validator)
        : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            var normalizedTypeNames = request.TypeNames
                .Select(n => n.ToLower().Trim())
                .Distinct()
                .ToArray();

            var typesToDelete = await typeRepository.AsQueryable()
                .Where(t => normalizedTypeNames.Contains(t.Name))
                .ToArrayAsync(cancellationToken);

            var notFoundTypes = normalizedTypeNames.Except(typesToDelete.Select(t => t.Name)).ToList();
            if (notFoundTypes.Count != 0)
                throw new TypeNotFoundException($"Category types not found: {string.Join(", ", notFoundTypes)}");

            await typeRepository.DeleteAsync(typesToDelete);
            await typeRepository.SaveChangesAsync(cancellationToken);
        }
    }
}