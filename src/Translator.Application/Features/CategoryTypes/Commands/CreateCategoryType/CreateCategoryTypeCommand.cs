using MediatR;

namespace Translator.Application.Features.CategoryTypes.Commands.CreateCategoryType;

public sealed record CreateCategoryTypeCommand(
    string TypeName) : IRequest<Guid>;