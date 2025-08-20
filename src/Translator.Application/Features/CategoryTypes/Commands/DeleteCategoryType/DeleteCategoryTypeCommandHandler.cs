using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Domain.DataModels;
using Translator.Infrastructure.Database.Postgres.Repository;

namespace Translator.Application.Features.CategoryTypes.Commands.DeleteCategoryType;

public class DeleteCategoryTypeCommandHandler(IRepository<CategoryType> typeRepository) : IRequestHandler<DeleteCategoryTypeCommand>
{
    public async Task Handle(DeleteCategoryTypeCommand request, CancellationToken cancellationToken)
    {
        var typeExists = await typeRepository.AsQueryable()
            .FirstOrDefaultAsync(t => t.Name == request.TypeName, cancellationToken: cancellationToken);
        
        if(typeExists is null)
            throw new TypeNotFoundException(request.TypeName);

        await typeRepository.DeleteAsync([typeExists]);
        await typeRepository.SaveChangesAsync(cancellationToken);
    }
}