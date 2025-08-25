using DistLib;
using DistLib.Domain;
using PFinance.Services.Identity.Common.Enums;

namespace PFinance.Services.Identity.Domain.User;

public class UserFactory : IUserFactory
{
    public Result<User> CreateUser(string email, string passwordHash, UserRole role, DateTime dateTime)
    {
        var verifiedEmail = Email.Create(email);
        if (verifiedEmail.IsFailure)
        {
            return Result.Failure<User>(verifiedEmail.Error);
        }
        
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return Result.Failure<User>(new Error("INVALID_PASSWORD", "Password is required"));
        }
        return User.Create(verifiedEmail.Value, passwordHash, role, dateTime);
    }
}