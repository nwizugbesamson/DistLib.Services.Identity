using DistLib;
using DistLib.Domain;
using PFinance.Services.Identity.Common.Enums;

namespace PFinance.Services.Identity.Domain.Role;

public class Role : Aggregate<AggregateId>
{
    private readonly List<Permission> _permissions = new();
    public UserRole Name { get; private set; }
    public IReadOnlyCollection<Permission> Permissions => _permissions.AsReadOnly();
    
    private Role() { }
    
    
    private Role(AggregateId id, UserRole name)
        => (Id, Name) = (id, name);
    
    public static Result<Role> Create(UserRole name, IEnumerable<(string action, string resource)> permissions)
    {

        Role role = new Role(AggregateId.New(), name);
        Result addPermissionResult = role.AddPermissions(permissions);
        return addPermissionResult.IsSuccess ? 
            Result.Success(role) : Result.Failure<Role>(addPermissionResult.Error);
    }
    
    public Result AddPermission(string action, string resource)
    {
        Result<Permission> permissionResult = Permission.Create(action, resource);
        if (permissionResult.IsFailure)
        {
            return Result.Failure(permissionResult.Error);
        }

        return _permissions.Contains(permissionResult.Value) 
            ? Result.Failure(new Error("DUPLICATE_PERMISSION", "This permission already exists.")) 
            : Result.Success();
    }
    
    public Result RemovePermission(string action, string resource)
    {
        Result<Permission> permissionResult = Permission.Create(action, resource);
        if (permissionResult.IsFailure)
        {
            return Result.Failure(permissionResult.Error);
        }

        var permission = permissionResult.Value;
        if (!_permissions.Contains(permission))
        {
            return Result.Failure(new Error("PERMISSION_NOT_FOUND", "The specified permission does not exist."));
        }

        _permissions.Remove(permission);
        return Result.Success();
    }
    
    public Result AddPermissions(IEnumerable<(string action, string resource)> permissions)
    {
        foreach (var (action, resource) in permissions)
        {
            var result = AddPermission(action, resource);
            if (result.IsFailure)
                return result; // fail fast
        }
        return Result.Success();
    }

    public Result RemovePermissions(IEnumerable<(string action, string resource)> permissions)
    {
        foreach (var (action, resource) in permissions)
        {
            var result = RemovePermission(action, resource);
            if (result.IsFailure)
                return result; // fail fast
        }
        return Result.Success();
    }


    
}