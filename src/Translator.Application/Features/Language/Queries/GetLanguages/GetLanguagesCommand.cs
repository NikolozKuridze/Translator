using MediatR;

namespace Translator.Application.Features.Language.Queries.GetLanguages;

public record GetLanguagesCommand() : IRequest<IEnumerable<GetLanguagesResponse>>;