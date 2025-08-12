using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Infrastructure.Database.Postgres.Repository;

using TemplateEntity = Translator.Domain.DataModels.Template;

namespace Translator.Application.Features.Template.Queries.GetAllTemplates;

public class GetAllTemplatesHandler : IRequestHandler<GetAllTemplatesCommand, IEnumerable<GetAllTemplatesResponse>>
{
    private readonly IRepository<TemplateEntity> _repository;

    public GetAllTemplatesHandler(IRepository<TemplateEntity> repository)
    {
        _repository = repository;
    }
    
    public async Task<IEnumerable<GetAllTemplatesResponse>> Handle(GetAllTemplatesCommand request, CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Min(Math.Max(1, request.PageSize), 100);

      
        var query = _repository
            .AsQueryable()
            .AsNoTracking();
        query = request.SortBy.ToLower() switch
        {
            "name" => request.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase) 
                ? query.OrderByDescending(t => t.Name) 
                : query.OrderBy(t => t.Name),
            "value" or "valuecount" => request.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase) 
                ? query.OrderByDescending(t => t.Values.Count) 
                : query.OrderBy(t => t.Values.Count),
            _ => query.OrderBy(t => t.Name)
        };
        
        var templateCount = await query.CountAsync(cancellationToken);
        
        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new GetAllTemplatesResponse(
                t.Name,
                t.Values.Count,
                templateCount
            ))
            .ToListAsync(cancellationToken);
    }
}

public record GetAllTemplatesResponse(
    string TemplateName, int ValueCount, int TemplateCount
);