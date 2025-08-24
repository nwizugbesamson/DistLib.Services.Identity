using DistLib;
using DistLib.Domain;

namespace PFinance.Services.Identity.Domain.Role;

public class Permission : ValueObject
{
    public string Action { get;  }
    public string Resource { get;  }

    private Permission(string action, string resource) 
        => (Action, Resource) = (action, resource);
    public static Result<Permission> Create(string action, string resource)
    {
        if (string.IsNullOrWhiteSpace(action) || string.IsNullOrWhiteSpace(resource))
        {
            return Result.Failure<Permission>(new Error("INVALID_PERMISSION_SPEC", ""));
        }

        return Result.Success(new Permission(action, resource));
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Action;
        yield return Resource;
    }
}