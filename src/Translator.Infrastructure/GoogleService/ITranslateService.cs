using Translator.Domain.Contracts;

namespace TestTranslateApp.Application.Services.TranslationService;

public interface ITranslationService
{
    Task<TranslateResponse> TranslateTextAsync(TranslateRequest request);
}

