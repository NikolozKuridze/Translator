using MediatR;
using Translator.Infrastructure.Database.Redis.CacheServices;

namespace Translator.Application.Features.Caching.Commands.CacheTemplate;

public record CacheTemplateCommand(Guid TemplateId, string TemplateName, List<TranslationDto> Values) : IRequest;