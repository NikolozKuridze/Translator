using MediatR;

namespace Translator.Application.Features.Template.Commands.DeleteTemplate;

public record DeleteTemplateCommand(string TemplateName) : IRequest;