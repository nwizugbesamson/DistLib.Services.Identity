using DistLib;

namespace PFinance.Services.Identity.Application.UserAccount.Commands.UseRefreshToken;

public class UseRefreshTokenCommandHandler : ICommandHandler<UseRefreshToken, Result<AuthDto>>
{
    public Task<Result<AuthDto>> Handle(UseRefreshToken request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}