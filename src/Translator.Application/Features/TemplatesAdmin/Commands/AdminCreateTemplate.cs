using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Configurations.Constants;
using Translator.Infrastructure.Database.Postgres.Repository;
using TemplateEntity = Translator.Domain.Entities.Template;

namespace Translator.Application.Features.TemplatesAdmin.Commands;

public abstract class AdminCreateTemplate
{
    public sealed record Command(
        string TemplateName,
        IEnumerable<string> Values) : IRequest;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.TemplateName)
                .NotEmpty()
                .WithMessage("Template name cannot be empty")
                .Length(3, DatabaseConstants.Template.TEMPLATE_NAME_MAX_LENGTH)
                .WithMessage(
                    $"Template name must be between 3 and {DatabaseConstants.Template.TEMPLATE_NAME_MAX_LENGTH} characters");

            RuleFor(x => x.Values)
                .NotEmpty()
                .WithMessage("Template must contain at least one value")
                .Must(values => values != null && values.Any())
                .WithMessage("Template cannot be empty");

            RuleForEach(x => x.Values)
                .NotEmpty()
                .WithMessage("Value name cannot be empty")
                .MaximumLength(DatabaseConstants.Value.KEY_MAX_LENGTH)
                .WithMessage($"Value name cannot exceed {DatabaseConstants.Value.KEY_MAX_LENGTH} characters");
        }
    }

    public class Handler : IRequestHandler<Command>
    {
        private readonly IRepository<TemplateEntity> _templateRepository;
        private readonly IValidator<Command> _validator;
        private readonly IRepository<Value> _valueRepository;

        public Handler(
            IRepository<TemplateEntity> templateRepository,
            IRepository<Value> valueRepository,
            IValidator<Command> validator)
        {
            _templateRepository = templateRepository;
            _valueRepository = valueRepository;
            _validator = validator;
        }

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            await _validator.ValidateAndThrowAsync(request, cancellationToken);

            var templateNameHash = TemplateEntity.HashName(request.TemplateName);

            var existsTemplate = await _templateRepository
                .Where(t => t.Hash == templateNameHash && t.OwnerId == null)
                .SingleOrDefaultAsync(cancellationToken);

            if (existsTemplate is not null)
                throw new TemplateAlreadyExistsException(existsTemplate.Id);

            var newTemplate = new TemplateEntity(request.TemplateName);

            var uniqueValues = request.Values.Distinct().ToList();
            var valueHashes = uniqueValues.Select(TemplateEntity.HashName).ToList();

            var availableValues = await _valueRepository
                .Where(v => valueHashes.Contains(v.Hash))
                .ToListAsync(cancellationToken);

            var missingValues = new List<string>();

            foreach (var valueName in uniqueValues)
            {
                var valueHash = TemplateEntity.HashName(valueName);
                var existsValue = availableValues.FirstOrDefault(v => v.Hash == valueHash);

                if (existsValue == null)
                {
                    missingValues.Add(valueName);
                    continue;
                }

                newTemplate.AddValue(existsValue);
            }

            if (missingValues.Count != 0)
            {
                var missingList = string.Join(", ", missingValues);
                throw new ValueNotFoundException($"Values not found: {missingList}");
            }

            if (newTemplate.Values.Count == 0)
                throw new InvalidOperationException("Template must contain at least one valid value");

            await _templateRepository.AddAsync(newTemplate, cancellationToken);
            await _templateRepository.SaveChangesAsync(cancellationToken);
        }
    }
}