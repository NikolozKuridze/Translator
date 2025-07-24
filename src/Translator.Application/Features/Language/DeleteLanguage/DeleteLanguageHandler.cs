using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Infrastructure.Database.Postgres.Repository;
using LanguageEntity = Translator.Domain.DataModels.Language;
namespace Translator.Application.Features.Language.DeleteLanguage;

public class DeleteLanguageHandler : IRequestHandler<DeleteLanguageCommand>
{
    private readonly IRepository<LanguageEntity> _repository;
    private readonly IValidator<DeleteLanguageCommand> _validator;

    public DeleteLanguageHandler(
        IRepository<LanguageEntity> repository,
        IValidator<DeleteLanguageCommand> validator)
    {
        _repository = repository;
        _validator = validator;
    }
    
    public async Task Handle(DeleteLanguageCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        
        var existsLanguage = await _repository
            .Where(l => l.Code == request.Code)
            .SingleOrDefaultAsync(cancellationToken);
        if (existsLanguage is null)
            throw new LanguageNotFoundException(request.Code);
        
        await _repository.DeleteAsync([existsLanguage]);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}