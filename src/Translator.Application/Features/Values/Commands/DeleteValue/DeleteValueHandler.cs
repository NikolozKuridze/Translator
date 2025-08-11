using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Domain.DataModels;
using Translator.Infrastructure.Database.Postgres.Repository;
using TemplateEntity = Translator.Domain.DataModels.Template;

namespace Translator.Application.Features.Values.Commands.DeleteValue;

public class DeleteValueHandler : IRequestHandler<DeleteValueCommand>
{
    private readonly IRepository<Value> _valueRepository;

    public DeleteValueHandler(
        IRepository<Value> valueRepository)
    {
        _valueRepository = valueRepository;
    }
    public async Task Handle(DeleteValueCommand request, CancellationToken cancellationToken)
    {
        var valueNameHash = TemplateEntity.HashName(request.ValueName);
        
        var templateValueExists = await _valueRepository
            .Where(t => t.Hash == valueNameHash) 
            .SingleOrDefaultAsync(cancellationToken);

        if (templateValueExists is null)
            throw new ValueNotFoundException(request.ValueName);

        await _valueRepository.DeleteAsync([templateValueExists]);
        await _valueRepository.SaveChangesAsync(cancellationToken);
    }
}