using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Contracts;
using Translator.Application.Exceptions;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Repository;
using ValueEntity = Translator.Domain.Entities.Value;

namespace Translator.Application.Features.Values.Queries;

public abstract class GetLandingValues
{
    public sealed record Query : IRequest<Response>;

    public sealed record Response
    {
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public Address Address { get; set; } = new();
        public string Email { get; set; } = string.Empty;
    }

    public sealed record Address
    {
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }

    public sealed class Handler(
        ICurrentUserService userService,
        IRepository<ValueEntity> valueRepository,
        IRepository<User> userRepository) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var userId = userService.GetCurrentUserId();
            if (!userId.HasValue)
                throw new UnauthorizedAccessException("User authentication required");

            var user = await userRepository
                .Where(u => u.Id == userId.Value)
                .SingleOrDefaultAsync(cancellationToken);

            if (user == null)
                throw new UserNotFoundException(userId.Value);

            var query = valueRepository
                .AsQueryable()
                .Include(v => v.Translations)
                .Where(v => v.OwnerId == userId.Value)
                .AsNoTracking();

            var values = await query.ToArrayAsync(cancellationToken);
            
            if (await query.CountAsync(cancellationToken) > 10)
                throw new Exception("This endpoint is not for you");
            
            var response = new Response();
                
            foreach (var value in values)
            {
                var firstTranslation = value.Translations.FirstOrDefault();
                if (firstTranslation == null) continue;

                switch (value.Key)
                {
                    // For Response
                    case "Name": 
                        response.Name = firstTranslation.TranslationValue;
                        break;
                    case "Email": 
                        response.Email = firstTranslation.TranslationValue;
                        break;
                    case "PhoneNumber": 
                        response.PhoneNumber = firstTranslation.TranslationValue;
                        break;
                        
                    // For Address
                    case "Street": 
                        response.Address.Street = firstTranslation.TranslationValue;
                        break;
                    case "PostalCode": 
                        response.Address.PostalCode = firstTranslation.TranslationValue;
                        break;
                    case "City": 
                        response.Address.City = firstTranslation.TranslationValue;
                        break;
                    case "Country": 
                        response.Address.Country = firstTranslation.TranslationValue;
                        break;
                }
            }
                    
            return response;
        }
    }
}
