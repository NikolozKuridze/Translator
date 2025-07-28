using Google.Cloud.Translation.V2;
using Translator.Domain.Contracts;
using Translator.Infrastructure.Database.Redis;

namespace Translator.Infrastructure.GoogleService
{
    public class GoogleTranslationService : ITranslationService
    {
        private readonly TranslationClient _translationClient;
        private readonly IRedisService _cacheService;

        public GoogleTranslationService(TranslationClient translationClient, IRedisService cacheService)
        {
            _translationClient = translationClient;  
            _cacheService = cacheService;
        }

        public async Task<TranslateResponse> TranslateTextAsync(TranslateRequest request)
        {
            var response = new TranslateResponse
            {
                OriginalText = request.Text,
                TargetLanguage = request.TargetLanguage
            };

            string targetLanguageCode = request.TargetLanguage.ToString();

            string cacheKey = $"translation:{request.Text.Trim().ToLower()}:{targetLanguageCode}";
            var value = await _cacheService.GetAsync<TranslateResponse>(cacheKey);

            if (value is not null)
                return value;
            
            TranslationResult translationResult = await _translationClient.TranslateTextAsync(
                request.Text,
                targetLanguageCode
            );

            response.DetectedLanguage = translationResult.DetectedSourceLanguage;
            response.TranslatedText = translationResult.TranslatedText;

            await _cacheService.SetAsync(cacheKey, response);
            
            return response;
        }
    }
}
