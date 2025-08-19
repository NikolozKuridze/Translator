using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Domain.DataModels;
using Translator.Infrastructure.Database.Postgres.Repository;

namespace Translator.Application.Features.CategoryTypes.Queries;

public class GetAllTypesQueryHandler(IRepository<CategoryType> typeRepository) : IRequestHandler<GetAllTypesQuery, List<string>>
{
    public async Task<List<string>> Handle(GetAllTypesQuery request, CancellationToken cancellationToken)
    {
        var types = await typeRepository.AsQueryable().ToListAsync(cancellationToken: cancellationToken);
        
        if(types.Count == 0)
            throw new Exception("No categories found.");

        List<string> typesAsStrings = [];
        
        typesAsStrings.AddRange(types.Select(t => t.Type));

        return typesAsStrings;
    }
}

