using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Configurations.Constants;
using Translator.Infrastructure.Database.Postgres.Repository;

namespace Translator.Application.Features.CategoryTypes.Commands;

public abstract class AddBulkCategoryTypes
{
    public sealed record Command(
        IEnumerable<string> TypeNames) : IRequest<Response>;

    public sealed record Response(
        IEnumerable<string> ExistingTypeNames,
        IEnumerable<string> CreatedTypeNames);

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
        : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            var normalizedTypeNames = request.TypeNames
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name.ToLower().Trim())
                .Distinct()
                .ToList();

            var existingTypes = await typeRepository.AsQueryable()
                .Where(t => normalizedTypeNames.Contains(t.Name))
                .Select(t => t.Name)
                .ToListAsync(cancellationToken);

            var typesToCreate = normalizedTypeNames
                .Except(existingTypes)
                .ToList();

            if (typesToCreate.Count == 0)
                return new Response(
                    ExistingTypeNames: existingTypes,
                    CreatedTypeNames: []);

            var newCategoryTypes = typesToCreate
                .Select(typeName => new CategoryType(typeName))
                .ToList();

            foreach (var categoryType in newCategoryTypes)
            {
                await typeRepository.AddAsync(categoryType, cancellationToken);
            }

            await typeRepository.SaveChangesAsync(cancellationToken);

            return new Response(
                ExistingTypeNames: existingTypes,
                CreatedTypeNames: typesToCreate);
        }
    }
}