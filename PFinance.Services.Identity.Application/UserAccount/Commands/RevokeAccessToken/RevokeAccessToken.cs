using DistLib;

namespace PFinance.Services.Identity.Application.UserAccount.Commands.RevokeAccessToken;

public sealed record RevokeAccessToken(string AccessToken) : ICommand<Result>;