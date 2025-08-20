using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Infrastructure.Database.Postgres.Repository;
using TemplateEntity = Translator.Domain.Entities.Template;

namespace Translator.Application.Features.Template.Commands.DeleteTemplate;

public class DeleteTemplateHandler : IRequestHandler<DeleteTemplateCommand>
{
    private readonly IRepository<TemplateEntity> _templateRepository;

    public DeleteTemplateHandler(IRepository<TemplateEntity> templateRepository)
        => _templateRepository = templateRepository;
    
    public async Task Handle(DeleteTemplateCommand request, CancellationToken cancellationToken)
    {
        var templateNameHash = TemplateEntity.HashName(request.TemplateName);
        var existsTemplate = await _templateRepository
            .Where(t => t.Hash == templateNameHash)
            .SingleOrDefaultAsync(cancellationToken);
        
        if (existsTemplate is null)
            throw new TemplateNotFoundException(request.TemplateName);
        
        await _templateRepository.DeleteAsync([existsTemplate]);
        await _templateRepository.SaveChangesAsync(cancellationToken);
    }
}