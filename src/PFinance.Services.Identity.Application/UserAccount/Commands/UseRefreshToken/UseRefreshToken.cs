using DistLib;

namespace PFinance.Services.Identity.Application.UserAccount.Commands.UseRefreshToken;

public sealed record UseRefreshToken( string RefreshToken) : ICommand<Result<AuthDto>>;