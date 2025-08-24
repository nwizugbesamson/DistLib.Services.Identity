using DistLib;

namespace PFinance.Services.Identity.Application.UserAccount.Commands.SignInPassword;

public class SignInPasswordCommandHandler : ICommandHandler<SignInPassword, Result<AuthDto>>
{
    public Task<Result<AuthDto>> Handle(SignInPassword request, CancellationToken cancellationToken)
    {
        // user repository find user id by email
        // => user dto || auth service.validate signin
        
        // check password -> password hasher
        
        // Generate token (email, role, permissions) - jwt service
        
        // return auth dto
        
        throw new NotImplementedException();
    }
}
