using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Repository;

namespace Translator.Application.Features.CategoryTypes.Commands.CreateBulkCategoryType;

public class CreateBulkCategoryTypeCommandHandler(
    IRepository<CategoryType> typeRepository)
    : IRequestHandler<CreateBulkCategoryTypeCommand, CreateBulkCategoryTypeResponse>
{
    public async Task<CreateBulkCategoryTypeResponse> Handle(CreateBulkCategoryTypeCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedTypeNames = request.TypeNames
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct()
            .ToList();

        if (normalizedTypeNames.Count == 0)
            return new CreateBulkCategoryTypeResponse(
                ExistingTypeNames: [],
                CreatedTypeNames: []);

        var existingTypes = await typeRepository.AsQueryable()
            .Where(t => normalizedTypeNames.Contains(t.Name))
            .Select(t => t.Name)
            .ToListAsync(cancellationToken);

        var typesToCreate = normalizedTypeNames
            .Except(existingTypes)
            .ToList();

        var createdTypeNames = new List<string>();

        if (typesToCreate.Count == 0)
            return new CreateBulkCategoryTypeResponse(
                ExistingTypeNames: existingTypes,
                CreatedTypeNames: createdTypeNames);

        var newCategoryTypes = typesToCreate
            .Select(typeName => new CategoryType(typeName))
            .ToList();

        foreach (var categoryType in newCategoryTypes)
        {
            await typeRepository.AddAsync(categoryType, cancellationToken);
            createdTypeNames.Add(categoryType.Name);
        }

        await typeRepository.SaveChangesAsync(cancellationToken);

        return new CreateBulkCategoryTypeResponse(
            ExistingTypeNames: existingTypes,
            CreatedTypeNames: createdTypeNames);
    }
}

public sealed record CreateBulkCategoryTypeResponse(
    IEnumerable<string> ExistingTypeNames,
    IEnumerable<string> CreatedTypeNames);