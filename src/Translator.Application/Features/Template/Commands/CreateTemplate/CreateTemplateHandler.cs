using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Infrastructure.Database.Postgres.Repository;
using TemplateEntity = Translator.Domain.DataModels.Template;

namespace Translator.Application.Features.Template.Commands.CreateTemplate;

public class CreateTemplateHandler : IRequestHandler<CreateTemplateCommand>
{
    private readonly IRepository<TemplateEntity> _templateRepository;

    public CreateTemplateHandler(
        IRepository<TemplateEntity> templateRepository)
    {
        _templateRepository = templateRepository;
    }
    
    public async Task Handle(CreateTemplateCommand request, CancellationToken cancellationToken)
    {
        var templateNameHash = TemplateEntity.HashName(request.TemplateName);
        
        var existsTemplate = await _templateRepository
            .Where(t => t.Hash == templateNameHash)
            .SingleOrDefaultAsync(cancellationToken);

        if (existsTemplate is not null)
            throw new TemplateAlreadyExistsException(request.TemplateName);

        var template = new TemplateEntity(request.TemplateName);
        
        await _templateRepository.AddAsync(template, cancellationToken);
        await _templateRepository.SaveChangesAsync(cancellationToken);
    }
}