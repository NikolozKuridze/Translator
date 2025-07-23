using MediatR;

namespace Translator.Application.Features.TemplateValue.Commands.CreateTemplateValue;

public record CreateTemplateValueCommand(
        string TemplateName,
        string Key, 
        string Value
    ) : IRequest;