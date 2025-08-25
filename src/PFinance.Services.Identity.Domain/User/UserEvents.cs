using DistLib;
using PFinance.Services.Identity.Common.Enums;

namespace PFinance.Services.Identity.Domain.User;

public static class UserEvents
{
    public static UserCreated UserCreated(Guid id, string email, UserRole role, DateTime createdAt) => 
        new (id, email, role, createdAt);
    
    public static UserSignedIn UserSignedIn(Guid id, string email, UserRole role) =>
        new (id, email, role);
    public static UserDeactivated UserDeactivated(Guid id, string email, UserRole role) =>
    new (id, email, role);
}

public record UserCreated(Guid Id, string Email, UserRole Role, DateTime CreatedAt) : DomainEventBase();
public record UserSignedIn(Guid Id, string Email, UserRole Role) : DomainEventBase();
public record UserDeactivated(Guid ApplicationId, string Email, UserRole Role) : DomainEventBase();