using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Repository;

namespace Translator.Application.Features.Users.Queries;

public abstract class GetUsers
{
    public sealed record Query : IRequest<IEnumerable<Response>>;
    public sealed record Response(
            Guid UserId,
            string UserName,
            string SecretKey
        );

    public class Handler(
        IRepository<User> userRepository
    ) : IRequestHandler<Query, IEnumerable<Response>>
    {
        public async Task<IEnumerable<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var users = await userRepository.AsQueryable()
                .Select(u => new Response(
                    u.Id,
                    u.Username,
                    u.SecretKey
                ))
                .ToListAsync(cancellationToken);

            return users;
        }
    }
}