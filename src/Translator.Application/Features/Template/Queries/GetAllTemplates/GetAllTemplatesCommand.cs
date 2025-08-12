using MediatR;

namespace Translator.Application.Features.Template.Queries.GetAllTemplates;

public record GetAllTemplatesCommand(int PageNumber = 1, int PageSize = 10,
    string SortBy = "name", string SortDirection = "asc") : IRequest<IEnumerable<GetAllTemplatesResponse>>;
