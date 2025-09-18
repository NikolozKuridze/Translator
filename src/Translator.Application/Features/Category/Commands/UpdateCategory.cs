using System.Text.Json;
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
        string? Metadata,
        string? Shortcode,
        int? Order) : IRequest;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Id cannot be empty.");

            RuleFor(x => x.Value)
                .Length(DatabaseConstants.Category.VALUE_MIN_LENGTH, DatabaseConstants.Category.VALUE_MAX_LENGTH);

            RuleFor(x => x.Order)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Order must be greater than or equal to 0.");

            RuleFor(x => x.Metadata)
                .Must(BeValidJson)
                .WithMessage("Metadata must be valid JSON.");

            RuleFor(x => x.Shortcode)
                .Length(DatabaseConstants.Category.VALUE_MIN_LENGTH, DatabaseConstants.Category.VALUE_MAX_LENGTH)
                .Matches("^[a-zA-Z]+$")
                .WithMessage("Shortcode must only contain letters.");

            RuleFor(x => x)
                .Must(x => x.Value != null || x.Order != null || x.Metadata != null || x.Shortcode != null)
                .WithMessage("Update at least one field.");
        }

        private bool BeValidJson(string? metadata)
        {
            if (string.IsNullOrWhiteSpace(metadata))
                return true;
            try
            {
                JsonDocument.Parse(metadata);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
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

            categoryToUpdate.Value = proposedValue.ToLower().Trim();
            categoryToUpdate.Order = request.Order;
            categoryToUpdate.Metadata = request.Metadata;
            categoryToUpdate.Shortcode = request.Shortcode?.ToLower().Trim();

            await categoryRepository.UpdateAsync(categoryToUpdate);
            await categoryRepository.SaveChangesAsync(cancellationToken);
        }
    }
}