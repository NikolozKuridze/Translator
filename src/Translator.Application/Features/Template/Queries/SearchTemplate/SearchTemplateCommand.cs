using MediatR;
using Translator.Application.Features.Template.Queries.GetAllTemplates;
using Translator.Domain.Pagination;

namespace Translator.Application.Features.Template.Queries.SearchTemplate;

public record SearchTemplateCommand(
    string TemplateName,
    PaginationRequest PaginationRequest) : IRequest<PaginatedResponse<GetAllTemplatesResponse>>;