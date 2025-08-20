using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Domain.Pagination;
using Translator.Infrastructure.Database.Postgres.Repository;

using TemplateEntity = Translator.Domain.DataModels.Template;

namespace Translator.Application.Features.Template.Queries.GetAllTemplates;

public class GetAllTemplatesHandler : IRequestHandler<GetAllTemplatesCommand, PaginatedResponse<GetAllTemplatesResponse>>
{
    private readonly IRepository<TemplateEntity> _repository;

    public GetAllTemplatesHandler(IRepository<TemplateEntity> repository)
        => _repository = repository;
    
    public async Task<PaginatedResponse<GetAllTemplatesResponse>> Handle(GetAllTemplatesCommand request, CancellationToken cancellationToken)
    {
        var query = _repository
            .AsQueryable()
            .AsNoTracking();

        query = request.Pagination.SortBy?.ToLower() switch
        {
            "name" => request.Pagination.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase) 
                ? query.OrderByDescending(t => t.Name) 
                : query.OrderBy(t => t.Name),
            "value" or "valuecount" => request.Pagination.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase) 
                ? query.OrderByDescending(t => t.Values.Count) 
                : query.OrderBy(t => t.Values.Count),
            _ => query.OrderBy(t => t.Name)
        };
        
        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Skip((request.Pagination.Page - 1) * request.Pagination.PageSize)
            .Take(request.Pagination.PageSize)
            .Select(t => new GetAllTemplatesResponse(
                t.Name,
                t.Id,
                t.Values.Count
            ))
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<GetAllTemplatesResponse>
        {
            Page = request.Pagination.Page,
            PageSize = request.Pagination.PageSize,
            TotalItems = totalCount,
            HasNextPage = (request.Pagination.Page * request.Pagination.PageSize) < totalCount,
            HasPreviousPage = request.Pagination.Page > 1,
            Items = items
        };
    }
}

public record GetAllTemplatesResponse(
    string TemplateName, 
    Guid TemplateId, 
    int ValueCount
);
