using MediatR;

namespace Translator.Application.Features.Caching.Commands.DeleteTemplateCache;

public record DeleteTemplateCacheCommand(Guid TemplateId) : IRequest;