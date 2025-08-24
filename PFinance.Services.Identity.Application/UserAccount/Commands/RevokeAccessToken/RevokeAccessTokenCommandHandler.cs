using DistLib;

namespace PFinance.Services.Identity.Application.UserAccount.Commands.RevokeAccessToken;

public class RevokeAccessTokenCommandHandler : ICommandHandler<RevokeAccessToken, Result>
{
    public Task<Result> Handle(RevokeAccessToken request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}