using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Configurations.Constants;
using Translator.Infrastructure.Database.Postgres.Repository;

namespace Translator.Application.Features.Users.Commands;

public class DeleteUser
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
            
            var normalizedUserName = request.UserName.Trim();
            
            var userExists = await userRepository.AsQueryable()
                .FirstOrDefaultAsync(u => u.Username.ToLower() == normalizedUserName.ToLower(), cancellationToken);
            
            if (userExists is  null)
                throw new UserNotFoundException(normalizedUserName);

            await userRepository.DeleteAsync([userExists]);
            await userRepository.SaveChangesAsync(cancellationToken);

            return new Response(userExists.Id);
        }
    }
}