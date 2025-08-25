using DistLib;
using DistLib.Domain;
using PFinance.Services.Identity.Common.Enums;

namespace PFinance.Services.Identity.Domain.User;

public class User : Aggregate<AggregateId>
{
    public Email Email { get; private set; }
    public string PasswordHash { get; private set; }
    public bool IsActive { get; private set; }
    public UserRole Role { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private User(AggregateId id, Email email, string passwordHash, UserRole role, DateTime createdAt)
    {
        Id = id;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        CreatedAt = createdAt;
    }

    internal static User Create(Email email, string passwordHash, UserRole role, DateTime dateTime)
    {
        User user = new User(AggregateId.New(), email, passwordHash, role, dateTime)
        {
            IsActive = true
        };
        user.AddEvent(UserEvents.UserCreated(user.Id.Value, user.Email.Value, user.Role, user.CreatedAt));
        return user;
    }

    // confirm signup -> bool

    public Result ConfirmSignIn()
    {
        if (!IsActive)
        {
            return Result.Failure(new Error("INACTIVE_ACCOUNT", "Sign in failed"));
        }

        AddEvent(UserEvents.UserSignedIn(Id.Value, Email.Value, Role));
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
        {
            return Result.Failure(new Error("ACCOUNT_ALREADY_DEACTIVATED", "User account is already deactivated"));
        }

        IsActive = false;
        AddEvent(UserEvents.UserDeactivated(Id.Value, Email.Value, Role));
        return Result.Success();
    }
}