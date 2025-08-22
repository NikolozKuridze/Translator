using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Configurations.Constants;
using Translator.Infrastructure.Database.Postgres.Repository;

namespace Translator.Application.Features.CategoryTypes.Commands;

public abstract class AddCategoryType
{
    public sealed record Command(string TypeName) : IRequest<Response>;

    public sealed record Response(Guid Id);
    
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.TypeName)
                .NotEmpty()
                .WithMessage("Type name is required.")
                .MinimumLength(DatabaseConstants.Type.NAME_MIN_LENGTH)
                .WithMessage($"Type must be at least {DatabaseConstants.Type.NAME_MIN_LENGTH} characters.")
                .MaximumLength(DatabaseConstants.Type.NAME_MAX_LENGTH)
                .WithMessage($"Type cannot be longer than {DatabaseConstants.Type.NAME_MAX_LENGTH} characters.")
                .Matches(@"^[a-zA-Z-]+$")
                .WithMessage("Type name can only contain letters and hyphens.");
        }
    }

    public class Handler(IRepository<CategoryType> typeRepository, IValidator<Command> validator) : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);
            
            var normalizedTypeName = request.TypeName.ToLower().Trim();
            
            var typeExists = await typeRepository.AsQueryable()
                .FirstOrDefaultAsync(t => t.Name == normalizedTypeName, cancellationToken);
        
            if(typeExists is not null)
                throw new TypeAlreadyExistsException(normalizedTypeName);
        
            var type = new CategoryType(normalizedTypeName);
            await typeRepository.AddAsync(type, cancellationToken);
            await typeRepository.SaveChangesAsync(cancellationToken);
        
            return new Response(type.Id);
        }
    }

}