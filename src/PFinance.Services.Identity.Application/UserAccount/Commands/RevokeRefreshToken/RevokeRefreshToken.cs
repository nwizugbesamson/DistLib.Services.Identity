using DistLib;

namespace PFinance.Services.Identity.Application.UserAccount.Commands.RevokeRefreshToken;

public sealed record RevokeRefreshToken(string RefreshToken) : ICommand<Result>;