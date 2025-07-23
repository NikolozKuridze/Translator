using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Infrastructure.Database.Postgres.Repository;
using TemplateEntity = Translator.Domain.DataModels.Template;
using TemplateValueEntity = Translator.Domain.DataModels.TemplateValue;

namespace Translator.Application.Features.TemplateValue.Commands.DeleteTemplateValue;

public class DeleteTemplateValueHandler : IRequestHandler<DeleteTemplateValueCommand>
{
    private readonly IRepository<TemplateEntity> _templateRepository;
    private readonly IRepository<TemplateValueEntity> _templateValueRepository;

    public DeleteTemplateValueHandler(
        IRepository<TemplateEntity> templateRepository,
        IRepository<TemplateValueEntity> templateValueRepository)
    {
        _templateRepository = templateRepository;
        _templateValueRepository = templateValueRepository;
    }
    public async Task Handle(DeleteTemplateValueCommand request, CancellationToken cancellationToken)
    {
        var templateNameHash = TemplateEntity.HashName(request.templateName);
        var templateValueNameHash = TemplateEntity.HashName(request.templateValueName);

        var templateExists = await _templateRepository
            .Where(t => t.Hash == templateNameHash)
            .AnyAsync(cancellationToken);

        var templateValueExists = await _templateValueRepository
            .Where(t => t.Hash == templateValueNameHash)
            .SingleOrDefaultAsync(cancellationToken);

        if (!templateExists)
            throw new TemplateNotFoundException(request.templateName);
        if (templateValueExists is null)
            throw new TemplateValueNotFoundException(request.templateValueName);

        await _templateValueRepository.DeleteAsync([templateValueExists]);
        await _templateValueRepository.SaveChangesAsync(cancellationToken);
    }
}