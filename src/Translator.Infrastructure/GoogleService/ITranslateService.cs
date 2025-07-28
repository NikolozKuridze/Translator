using Translator.Domain.Contracts;

namespace Translator.Infrastructure.GoogleService;

public interface ITranslationService
{
    Task<TranslateResponse> TranslateTextAsync(TranslateRequest request);
}

