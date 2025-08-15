using MediatR;

namespace Translator.Application.Features.Caching.Commands.DeleteValueCache;

public record DeleteCommandCacheCommand(Guid ValueId) : IRequest;