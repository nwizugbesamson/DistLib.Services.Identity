using DistLib;
using PFinance.Services.Identity.Common.Enums;

namespace PFinance.Services.Identity.Application.UserAccount.Commands.CreateUser;

public sealed record CreateUser(Guid UserId, string Email, string Password, UserRole Role) : ICommand<Result>;