using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Repository;

namespace Translator.Application.Features.CategoryTypes.Commands.CreateCategoryType;

public class CreateCategoryTypeCommandHandler(
    IRepository<CategoryType> typeRepository) : IRequestHandler<CreateCategoryTypeCommand, Guid>
{
    public async Task<Guid> Handle(CreateCategoryTypeCommand request, CancellationToken cancellationToken)
    {
        var typeExists = await typeRepository.AsQueryable()
            .FirstOrDefaultAsync(t => t.Name == request.TypeName, cancellationToken: cancellationToken);
        
        if(typeExists is not null)
            throw new TypeAlreadyExistsException(request.TypeName);
        
        var type = new CategoryType(request.TypeName);
        await typeRepository.AddAsync(type, cancellationToken);
        await typeRepository.SaveChangesAsync(cancellationToken);
        
        return type.Id;
    }
}