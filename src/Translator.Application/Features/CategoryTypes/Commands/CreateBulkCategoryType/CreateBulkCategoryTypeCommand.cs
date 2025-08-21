using MediatR;

namespace Translator.Application.Features.CategoryTypes.Commands.CreateBulkCategoryType;

public sealed record CreateBulkCategoryTypeCommand(
    IEnumerable<string> TypeNames) : IRequest<CreateBulkCategoryTypeResponse>;