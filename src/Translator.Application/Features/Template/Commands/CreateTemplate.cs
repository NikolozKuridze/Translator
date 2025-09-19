using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Contracts.Infrastructure;
using Translator.Application.Exceptions;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Configurations.Constants;
using Translator.Infrastructure.Database.Postgres.Repository;
using TemplateEntity = Translator.Domain.Entities.Template;

namespace Translator.Application.Features.Template.Commands;

public abstract class CreateTemplate
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
        private readonly ICurrentUserService _currentUserService;
        private readonly IRepository<TemplateEntity> _templateRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IValidator<Command> _validator;
        private readonly IRepository<Value> _valueRepository;

        public Handler(
            ICurrentUserService currentUserService,
            IRepository<TemplateEntity> templateRepository,
            IRepository<Value> valueRepository,
            IRepository<User> userRepository,
            IValidator<Command> validator)
        {
            _currentUserService = currentUserService;
            _templateRepository = templateRepository;
            _valueRepository = valueRepository;
            _userRepository = userRepository;
            _validator = validator;
        }

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            await _validator.ValidateAndThrowAsync(request, cancellationToken);

            var userId = _currentUserService.GetCurrentUserId();
            if (!userId.HasValue)
                throw new UnauthorizedAccessException("User authentication required");

            var user = await _userRepository
                .Where(u => u.Id == userId.Value)
                .SingleOrDefaultAsync(cancellationToken);

            if (user == null)
                throw new UserNotFoundException(userId.Value);

            var templateNameHash = TemplateEntity.HashName(request.TemplateName);

            var existsTemplate = await _templateRepository
                .Where(t => t.Hash == templateNameHash && t.OwnerId == userId.Value)
                .SingleOrDefaultAsync(cancellationToken);

            if (existsTemplate is not null)
                throw new TemplateAlreadyExistsException(existsTemplate.Id);

            var newTemplate = new TemplateEntity(request.TemplateName, userId.Value);

            var uniqueValues = request.Values.Distinct().ToList();
            var valueHashes = uniqueValues.Select(TemplateEntity.HashName).ToList();

            var availableValues = await _valueRepository
                .Where(v => valueHashes.Contains(v.Hash) &&
                            (v.OwnerId == userId.Value || v.OwnerId == null))
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

                if (existsValue.OwnerId != null && existsValue.OwnerId != userId.Value)
                    throw new UnauthorizedOperationException($"Access denied to value '{valueName}'");

                newTemplate.AddValue(existsValue);
            }

            if (missingValues.Count != 0)
            {
                var missingList = string.Join(", ", missingValues);
                throw new ValueNotFoundException($"Values not found or access denied: {missingList}");
            }

            if (newTemplate.Values.Count == 0)
                throw new InvalidOperationException("Template must contain at least one valid value");

            await _templateRepository.AddAsync(newTemplate, cancellationToken);
            await _templateRepository.SaveChangesAsync(cancellationToken);
        }
    }
}