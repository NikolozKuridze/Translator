using System.Collections.Immutable;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Infrastructure.Database.Postgres.Repository;

using LanguageEntity = Translator.Domain.Entities.Language;

namespace Translator.Application.Features.Language.Queries.GetLanguages;

public class GetLanguagesHandler : IRequestHandler<GetLanguagesCommand, IEnumerable<GetLanguagesResponse>>
{
    private readonly IRepository<LanguageEntity> _repository;

    public GetLanguagesHandler(IRepository<LanguageEntity> repository) 
        => _repository = repository;
    
    public async Task<IEnumerable<GetLanguagesResponse>> Handle(GetLanguagesCommand request, CancellationToken cancellationToken)
    {
        return
            await _repository
                .AsQueryable()
                .IgnoreQueryFilters()
                .Select(l => new GetLanguagesResponse(
                    l.Code, l.Name, l.UnicodeRange, l.IsActive
                ))
                .ToArrayAsync(cancellationToken);
    }
}
public record GetLanguagesResponse(string LanguageCode, string LanguageName, string UnicodeRange, bool IsActive);