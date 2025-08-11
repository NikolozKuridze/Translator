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

        return await _repository
            .AsQueryable()
            .AsNoTracking()
            .OrderBy(t => t.Name) 
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new GetAllTemplatesResponse(
                t.Name,
                t.Values.Count
            ))
            .ToListAsync(cancellationToken);
    }
}

public record GetAllTemplatesResponse(
    string TemplateName, int ValueCount
);