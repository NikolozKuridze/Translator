using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Infrastructure.Database.Postgres.Repository;
using ValueEntity = Translator.Domain.DataModels.Value;

namespace Translator.Application.Features.Values.Queries.GetAllValues;

public class GetAllValuesHandler :  IRequestHandler<GetAllValuesCommand, IEnumerable<GetAllValuesResponse>>
{
    private readonly IRepository<ValueEntity> _valueRepository;

    public GetAllValuesHandler(
        IRepository<ValueEntity> valueRepository)
    {
        _valueRepository = valueRepository;
    }
    
    public async Task<IEnumerable<GetAllValuesResponse>> Handle(GetAllValuesCommand request, CancellationToken cancellationToken)
    {
        var query = _valueRepository
            .AsQueryable()
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);
        
        if (request.SortBy.ToLower() == "date")
            query = request.SortDirection.ToLower() == "desc"
                ? query.OrderByDescending(v => v.CreatedAt)
                : query.OrderBy(v => v.CreatedAt);
        
        return await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new GetAllValuesResponse(
                t.Key, 
                t.Translations.Count, 
                t.CreatedAt,
                totalCount))
            .ToArrayAsync(cancellationToken);
    }
}

public record GetAllValuesResponse(string Key, int TranslationsCount, DateTimeOffset CreatedAt, int TotalCount);