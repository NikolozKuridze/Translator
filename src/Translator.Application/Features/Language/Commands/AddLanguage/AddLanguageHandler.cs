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

        var existsLanguage = await _repository
            .Where(x => 
                x.Code == request.Code
                && x.IsActive == false)
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(cancellationToken);
        
        if (existsLanguage is null)
            throw new LanguageAlreadyAdded(request.Code);
        
        existsLanguage.IsActive = true;
        await _repository.SaveChangesAsync(cancellationToken);
        
        return new AddLanguageResponse(request.Code);
    }
}

public record AddLanguageResponse(string LanguageCode);