using MediatR;

namespace Translator.Application.Features.TemplateValue.Commands.DeleteTemplateValue;

public record DeleteTemplateValueCommand(
        string templateName,
        string templateValueName
    ) : IRequest;