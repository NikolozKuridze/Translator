using MediatR;

namespace Translator.Application.Features.Values.Queries.GetValue;

public record GetValueCommand(
        string ValueName,
        string? LanguageCode,
        bool AllTranslations
    ) : IRequest<IEnumerable<GetValueResponse>>;  