using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Configurations.Constants;
using Translator.Infrastructure.Database.Postgres.Repository;
using CategoryEntity = Translator.Domain.Entities.Category;

namespace Translator.Application.Features.Category.Commands;

public abstract class AddCategory
{
    public sealed record Command(
        string Value,
        string TypeName,
        int? Order,
        Guid? ParentId
    ) : IRequest<Response>;

    public sealed record Response(Guid Id);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Value)
                .NotEmpty()
                .WithMessage("Value cannot be empty.")
                .Length(DatabaseConstants.Category.VALUE_MIN_LENGTH, DatabaseConstants.Category.VALUE_MAX_LENGTH);
        }
    }

    public class Handler(
        IRepository<CategoryEntity> categoryRepository,
        IRepository<CategoryType> typeRepository,
        IValidator<Command> validator) : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            if (request.ParentId.HasValue)
            {
                var parent = await categoryRepository.AsQueryable()
                    .Include(p => p.Type)
                    .FirstOrDefaultAsync(p => p.Id == request.ParentId.Value, cancellationToken);

                if (parent is null)
                    throw new CategoryNotFoundException(request.ParentId.Value);

                if (parent.Type.Name.Equals(request.TypeName, StringComparison.OrdinalIgnoreCase))
                    throw new SameTypeException();
            }

            var typeExists = await typeRepository.AsQueryable()
                .FirstOrDefaultAsync(t =>
                        t.Name == request.TypeName
                            .ToLower()
                            .Trim(),
                    cancellationToken);

            if (typeExists is null) throw new TypeNotFoundException(request.TypeName);

            var category = new CategoryEntity(
                request.Value.ToLower().Trim(),
                typeExists.Id,
                request.Order,
                request.ParentId);

            await categoryRepository.AddAsync(category, cancellationToken);
            await categoryRepository.SaveChangesAsync(cancellationToken);

            return new Response(category.Id);
        }
    }
}