using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Features.Template.Queries.GetAllTemplates;
using Translator.Domain.Pagination;
using Translator.Infrastructure.Database.Postgres.Repository;

using TemplateEntity = Translator.Domain.Entities.Template;

namespace Translator.Application.Features.Template.Queries.SearchTemplate;

public class SearchTemplateHandler : IRequestHandler<SearchTemplateCommand, PaginatedResponse<GetAllTemplatesResponse>>
{
    private readonly IRepository<TemplateEntity> _templateRepository;

    public SearchTemplateHandler(IRepository<TemplateEntity> templateRepository)
        => _templateRepository = templateRepository;
    
    public async Task<PaginatedResponse<GetAllTemplatesResponse>> Handle(SearchTemplateCommand request, CancellationToken cancellationToken)
    { 
        var query = _templateRepository
            .Where(v =>
                string.IsNullOrEmpty(request.TemplateName)||
                v.Name.Contains(request.TemplateName) ||
                v.Name == request.TemplateName);

        var totalItems = await query.CountAsync(cancellationToken);

        var templates = await query
            .Skip((request.PaginationRequest.Page - 1) * request.PaginationRequest.PageSize)
            .Take(request.PaginationRequest.PageSize)
            .Select(tk => new GetAllTemplatesResponse(
                tk.Name, tk.Id, tk.Values.Count
            ))
            .ToArrayAsync(cancellationToken);

        return new PaginatedResponse<GetAllTemplatesResponse>()
        {
            Page = request.PaginationRequest.Page,
            PageSize = request.PaginationRequest.PageSize,
            TotalItems = totalItems,
            HasNextPage = request.PaginationRequest.Page * request.PaginationRequest.PageSize < totalItems,
            HasPreviousPage = request.PaginationRequest.Page > 1,
            Items = templates
        };
    }
}