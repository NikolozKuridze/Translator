using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Application.Features.Category.Commands;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Configurations.Constants;
using Translator.Infrastructure.Database.Postgres.Repository;

namespace Translator.Application.Features.Users.Commands;

public abstract class AddUser
{
    public sealed record Command(string UserName) : IRequest<Response>;

    public sealed record Response(Guid Id);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x =>  x.UserName)
                .NotEmpty()
                .MinimumLength(DatabaseConstants.User.USERNAME_MIN_LENGTH)
                .MaximumLength(DatabaseConstants.User.USERNAME_MAX_LENGTH);
        }
    }

    public class Handler(
        IRepository<User> userRepository,
        IValidator<Command> validator
        ) : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(request,  cancellationToken);

            var normalizedUserName = request.UserName.ToLower().Trim();
            
            var userExists = await userRepository.AsQueryable()
                .FirstOrDefaultAsync(u => u.Username == normalizedUserName, cancellationToken);
            
            if (userExists is not null)
                throw new UserAlreadyExistsException(normalizedUserName);

            var user = new User(normalizedUserName);
            await userRepository.AddAsync(user, cancellationToken);
            await userRepository.SaveChangesAsync(cancellationToken);

            return new Response(user.Id);
        }
    }
}