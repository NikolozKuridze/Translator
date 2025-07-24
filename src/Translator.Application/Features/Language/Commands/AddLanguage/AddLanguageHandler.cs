using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Infrastructure.Database.Postgres.Repository;
using LanguageEntity = Translator.Domain.DataModels.Language;

namespace Translator.Application.Features.Language.Commands.AddLanguage;

public class AddLanguageHandler : IRequestHandler<AddLanguageCommand, AddLanguageResponse>
{
    private readonly IRepository<LanguageEntity> _repository;
    private readonly IValidator<AddLanguageCommand> _validator;

    public AddLanguageHandler(
        IRepository<LanguageEntity> repository,
        IValidator<AddLanguageCommand> validator)
    {
        _repository = repository;
        _validator = validator;
    }
    
    public async Task<AddLanguageResponse> Handle(AddLanguageCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var existsTranslation = await _repository
            .Where(x => x.Code == request.Code)
            .SingleOrDefaultAsync(cancellationToken);
        
        if (existsTranslation is not null)
            throw new LanguageAlreadyExistsException(request.Name);
        
        var language = new LanguageEntity(request.Code, request.Name, request.UnicodeRange);
       
        await _repository.AddAsync(language, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return new AddLanguageResponse(request.Code, request.Name);
    }
}

public record AddLanguageResponse(string LanguageCode, string LanguageName);