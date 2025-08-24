using DistLib;

namespace PFinance.Services.Identity.Application.UserAccount.Commands.CreateUser;

public class CreateUserCommandHandler : ICommandHandler<CreateUser, Result>
{
    public Task<Result> Handle(CreateUser request, CancellationToken cancellationToken)
    {
        // check that user account with the email does not exist
        
        // hash password
        
        // create a user account with a factory -> raises an event
        
        // save with repository
        
        // complete transaction with unit of work
        throw new NotImplementedException();
    }
}