using MediatR;

namespace Translator.Application.Features.Template.Commands.CreateTemplate;

public record CreateTemplateCommand(string TemplateName) : IRequest;
