using System.Globalization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Repository;

namespace Translator.Application.Features.CategoryTypes.Queries;

public class GetAllTypesQueryHandler(IRepository<CategoryType> typeRepository) : IRequestHandler<GetAllTypesQuery, List<string>>
{
    public async Task<List<string>> Handle(GetAllTypesQuery request, CancellationToken cancellationToken)
    {
        var types = await typeRepository.AsQueryable()
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken: cancellationToken);
        
        List<string> typesAsStrings = [];
        
        var textInfo = CultureInfo.CurrentCulture.TextInfo;

        var capitalizedTypes = types.Select(t => textInfo.ToTitleCase(t.Name.ToLower())).ToList();

        return capitalizedTypes;
    }
}

