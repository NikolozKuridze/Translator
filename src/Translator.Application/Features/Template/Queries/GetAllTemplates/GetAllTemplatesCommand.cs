using MediatR;
using Translator.Domain.Pagination;

namespace Translator.Application.Features.Template.Queries.GetAllTemplates;

public record GetAllTemplatesCommand(PaginationRequest Pagination) : IRequest<PaginatedResponse<GetAllTemplatesResponse>>;