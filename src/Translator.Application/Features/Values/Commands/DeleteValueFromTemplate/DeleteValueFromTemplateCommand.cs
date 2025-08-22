using MediatR;

namespace Translator.Application.Features.Values.Commands.DeleteValueFromTemplate;

public record DeleteValueFromTemplateCommand(string ValueName, Guid TemplateId) : IRequest;