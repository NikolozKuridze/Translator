using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Infrastructure.Database.Postgres.Configurations.Constants;
using Translator.Infrastructure.Database.Postgres.Repository;
using CategoryEntity = Translator.Domain.Entities.Category;

namespace Translator.Application.Features.Category.Commands;

public abstract class UpdateCategory
{
    public sealed record Command(
        Guid Id,
        string? Value,
        int? Order) : IRequest;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Id cannot be empty.");

            When(x => x.Value != null, () =>
            {
                RuleFor(x => x.Value)
                    .NotEmpty()
                    .WithMessage("Value cannot be empty when provided.")
                    .Length(DatabaseConstants.Category.VALUE_MIN_LENGTH, DatabaseConstants.Category.VALUE_MAX_LENGTH);
            });

            When(x => x.Order != null, () =>
            {
                RuleFor(x => x.Order)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Order must be greater than or equal to 0.");
            });

            RuleFor(x => x)
                .Must(x => x.Value != null || x.Order != null)
                .WithMessage("At least one field (Value or Order) must be provided for update.");
        }
    }

    public class Handler(
        IRepository<CategoryEntity> categoryRepository,
        IValidator<Command> validator) : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            var categoryToUpdate = await categoryRepository
                .AsQueryable()
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (categoryToUpdate is null)
                throw new CategoryNotFoundException(request.Id);

            var proposedValue = request.Value ?? categoryToUpdate.Value;
            var proposedOrder = request.Order ?? categoryToUpdate.Order;

            categoryToUpdate.Value = proposedValue.ToLower().Trim();
            categoryToUpdate.Order = proposedOrder;

            await categoryRepository.UpdateAsync(categoryToUpdate);
            await categoryRepository.SaveChangesAsync(cancellationToken);
        }
    }
}