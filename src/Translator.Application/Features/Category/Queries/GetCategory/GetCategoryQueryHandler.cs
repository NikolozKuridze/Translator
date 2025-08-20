using System.Globalization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Infrastructure.Database.Postgres;
using Translator.Infrastructure.Database.Postgres.Repository;
using CategoryEntity = Translator.Domain.DataModels.Category;

namespace Translator.Application.Features.Category.Queries.GetCategory;

public class GetCategoryQueryHandlerRecursive(IRepository<CategoryEntity> categoryRepository)
    : IRequestHandler<GetCategoryQuery, CategoryReadDto>
{
    public async Task<CategoryReadDto> Handle(GetCategoryQuery request, CancellationToken cancellationToken)
    {
        var category = await categoryRepository
            .AsQueryable()
            .Include(c => c.Type)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category == null)
            throw new CategoryNotFoundException(request.Id);

        return await MapToDtoWithChildren(category, cancellationToken);
    }

    private async Task<CategoryReadDto> MapToDtoWithChildren(CategoryEntity category,
        CancellationToken cancellationToken)
    {
        var textInfo = CultureInfo.CurrentCulture.TextInfo;

        var children = await categoryRepository
            .AsQueryable()
            .Include(c => c.Type)
            .Where(c => c.ParentId == category.Id)
            .OrderBy(c => c.Order)
            .ToListAsync(cancellationToken);

        var childDtos = new List<CategoryReadDto>();
        foreach (var child in children)
        {
            var childDto = await MapToDtoWithChildren(child, cancellationToken);
            childDtos.Add(childDto);
        }

        return new CategoryReadDto(
            category.Id,
            textInfo.ToTitleCase(category.Value),
            textInfo.ToTitleCase(category.Type.Name),
            category.Order,
            category.ParentId,
            childDtos
        );
    }
}

public record CategoryReadDto(
    Guid Id,
    string Value,
    string TypeName,
    int? Order,
    Guid? ParentId,
    List<CategoryReadDto> Children
);