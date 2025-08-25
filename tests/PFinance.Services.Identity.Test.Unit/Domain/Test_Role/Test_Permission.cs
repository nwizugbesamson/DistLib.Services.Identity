using DistLib;
using RoleDomain = PFinance.Services.Identity.Domain.Role;
using Shouldly;

namespace PFinance.Services.Identity.Test.Unit.Domain.Test_Role;

public class Test_Permission
{
    [Theory]
    [InlineData("READ", "USERS")]
    [InlineData("WRITE", "POSTS")]
    [InlineData("DELETE", "COMMENTS")]
    [InlineData("CREATE", "ROLES")]
    [InlineData("UPDATE", "SETTINGS")]
    public void Test_PermissionIsCreatedSuccessfully_WhenDataIsValid(string action, string resource)
    {
        // Act
        Result<RoleDomain.Permission> result = RoleDomain.Permission.Create(action, resource);
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Action.ShouldBe(action);
        result.Value.Resource.ShouldBe(resource);
    }

    [Theory]
    [InlineData("", "USERS")]
    [InlineData(null, "POSTS")]
    [InlineData("READ", "")]
    [InlineData("WRITE", null)]
    [InlineData("", "")]
    [InlineData(null, null)]
    [InlineData("   ", "USERS")]
    [InlineData("READ", "   ")]
    public void Test_PermissionIsNotCreated_WhenActionOrResourceIsInvalid(string action, string resource)
    {
        // Act
        Result<RoleDomain.Permission> result = RoleDomain.Permission.Create(action, resource);
        
        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error.Code.ShouldBe("INVALID_PERMISSION_SPEC");
    }
    
}