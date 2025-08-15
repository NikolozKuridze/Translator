using MediatR;
using Translator.Infrastructure.Database.Redis.CacheServices;

namespace Translator.Application.Features.Caching.Commands.CacheValue;

public record CacheValueCommand(Guid ValueId, string ValueKey, List<TranslationDto> Translations) : IRequest;