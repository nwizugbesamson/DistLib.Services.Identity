using DistLib;

namespace PFinance.Services.Identity.Application.UserAccount.Commands.RevokeRefreshToken;

public class RevokeRefreshTokenCommandHandler : ICommandHandler<RevokeRefreshToken, Result>
{
    public Task<Result> Handle(RevokeRefreshToken request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}