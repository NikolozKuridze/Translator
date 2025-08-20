using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Repository;
using TemplateEntity = Translator.Domain.Entities.Template;

namespace Translator.Application.Features.Template.Commands.CreateTemplate;

public class CreateTemplateHandler : IRequestHandler<CreateTemplateCommand>
{
    private readonly IRepository<TemplateEntity> _templateRepository;
    private readonly IRepository<Value> _valueRepository;
    private readonly IValidator<CreateTemplateCommand> _validator;

    public CreateTemplateHandler(
        IRepository<TemplateEntity> templateRepository,
        IRepository<Value> valueRepository,
        IValidator<CreateTemplateCommand> validator)
    {
        _templateRepository = templateRepository;
        _valueRepository = valueRepository;
        _validator = validator;
    }
    
    public async Task Handle(CreateTemplateCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        
        var templateNameHash = TemplateEntity.HashName(request.TemplateName);
        
        var existsTemplate = await _templateRepository
            .Where(t => t.Hash == templateNameHash)
            .SingleOrDefaultAsync(cancellationToken);

        if (existsTemplate is not null)
            throw new TemplateAlreadyExistsException(existsTemplate.Id);

        var newTemplate = new TemplateEntity(request.TemplateName);
        
        foreach (var value in request.Values)
        {
            var valueHash = TemplateEntity.HashName(value);
            var existsValue = await _valueRepository
                .Where(v => v.Hash == valueHash)
                .SingleOrDefaultAsync(cancellationToken);
            if (existsValue is null)
                throw new ValueNotFoundException(valueHash);
            newTemplate.AddValue(existsValue);
        }
        
        await _templateRepository.AddAsync(newTemplate, cancellationToken);
        await _templateRepository.SaveChangesAsync(cancellationToken);
    }
}