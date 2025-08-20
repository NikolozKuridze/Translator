using MediatR;
using Translator.Domain.Pagination;

namespace Translator.Application.Features.Template.Queries.GetTemplate;

public record GetTemplateCommand(
    Guid TemplateId, 
    string? LanguageCode, 
    bool AllTranslates,
    PaginationRequest? Pagination) 
    : IRequest<PaginatedResponse<ValueDto>>;