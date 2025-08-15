using MediatR;

namespace Translator.Application.Features.Values.Queries.GetValue;

public record GetValueCommand(
        Guid ValueId,
        string? LanguageCode,
        bool AllTranslations
    ) : IRequest<IEnumerable<GetValueResponse>>;  