using MediatR;
using Translator.Infrastructure.Database.Redis.CacheServices;

namespace Translator.Application.Features.Caching.Commands.CacheTemplate;


public record CacheTemplateCommand(
    Guid TemplateId, 
    string TemplateName, 
    Guid? OwnerId,
    string? OwnerName,
    List<TranslationDto> Values) : IRequest;