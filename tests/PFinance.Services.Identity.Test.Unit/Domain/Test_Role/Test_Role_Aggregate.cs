using DistLib;
using RoleDomain = PFinance.Services.Identity.Domain.Role;
using PFinance.Services.Identity.Common.Enums;
using Shouldly;

namespace PFinance.Services.Identity.Test.Unit.Domain.Test_Role;

public class Test_Role_Aggregate
{
    [Theory]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.Customer)]
    public void Test_RoleIsCreatedSuccessfully_WhenDataIsValid(UserRole userRole)
    {
        // Arrange
        var permissions = new List<(string action, string resource)>
        {
            ("READ", "USERS"),
            ("WRITE", "POSTS")
        };
        
        // Act
        Result<RoleDomain.Role> result = RoleDomain.Role.Create(userRole, permissions);
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldNotBeNull();
        result.Value.Name.ShouldBe(userRole);
        result.Value.Permissions.Count.ShouldBe(2);
        result.Value.Permissions.Any(p => p is { Action: "READ", Resource: "USERS" }).ShouldBeTrue();
        result.Value.Permissions.Any(p => p is { Action: "WRITE", Resource: "POSTS" }).ShouldBeTrue();
    }

    [Theory]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.Customer)]
    public void Test_RoleIsCreatedSuccessfully_WhenNoPermissionsProvided(UserRole userRole)
    {
        // Arrange
        var permissions = new List<(string action, string resource)>();
        
        // Act
        Result<RoleDomain.Role> result = RoleDomain.Role.Create(userRole, permissions);
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Name.ShouldBe(userRole);
        result.Value.Permissions.Count.ShouldBe(0);
    }

    [Theory]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.Customer)]
    public void Test_RoleCreationFails_WhenInvalidPermissionsProvided(UserRole userRole)
    {
        // Arrange
        var permissions = new List<(string action, string resource)>
        {
            ("READ", "USERS"),
            ("", "POSTS") // Invalid permission
        };
        
        // Act
        Result<RoleDomain.Role> result = RoleDomain.Role.Create(userRole, permissions);
        
        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error.Code.ShouldBe("INVALID_PERMISSION_SPEC");
    }

    [Fact]
    public void Test_AddPermissionSucceeds_WhenValidPermissionProvided()
    {
        // Arrange
        var role = RoleDomain.Role.Create(UserRole.Admin, new List<(string action, string resource)>()).Value;
        
        // Act
        Result result = role.AddPermission("READ", "USERS");
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        role.Permissions.Count.ShouldBe(1);
        role.Permissions.First().Action.ShouldBe("READ");
        role.Permissions.First().Resource.ShouldBe("USERS");
    }

    [Fact]
    public void Test_AddPermissionFails_WhenInvalidPermissionProvided()
    {
        // Arrange
        var role = RoleDomain.Role.Create(UserRole.Admin, new List<(string action, string resource)>()).Value;
        
        // Act
        Result result = role.AddPermission("", "USERS");
        
        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("INVALID_PERMISSION_SPEC");
        role.Permissions.Count.ShouldBe(0);
    }

    [Fact]
    public void Test_AddPermissionFails_WhenDuplicatePermissionProvided()
    {
        // Arrange
        var role = RoleDomain.Role.Create(UserRole.Admin, new List<(string action, string resource)>()).Value;
        role.AddPermission("READ", "USERS");
        
        // Act
        Result result = role.AddPermission("READ", "USERS");
        
        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("DUPLICATE_PERMISSION");
        role.Permissions.Count.ShouldBe(1);
    }

    [Fact]
    public void Test_RemovePermissionSucceeds_WhenPermissionExists()
    {
        // Arrange
        var role = RoleDomain.Role.Create(UserRole.Admin, new List<(string action, string resource)>()).Value;
        role.AddPermission("READ", "USERS");
        role.AddPermission("WRITE", "POSTS");
        
        // Act
        Result result = role.RemovePermission("READ", "USERS");
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        role.Permissions.Count.ShouldBe(1);
        role.Permissions.First().Action.ShouldBe("WRITE");
        role.Permissions.First().Resource.ShouldBe("POSTS");
    }

    [Fact]
    public void Test_RemovePermissionFails_WhenPermissionDoesNotExist()
    {
        // Arrange
        var role = RoleDomain.Role.Create(UserRole.Admin, new List<(string action, string resource)>()).Value;
        role.AddPermission("READ", "USERS");
        
        // Act
        Result result = role.RemovePermission("WRITE", "POSTS");
        
        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("PERMISSION_NOT_FOUND");
        role.Permissions.Count.ShouldBe(1);
    }

    [Fact]
    public void Test_RemovePermissionFails_WhenInvalidPermissionProvided()
    {
        // Arrange
        var role = RoleDomain.Role.Create(UserRole.Admin, new List<(string action, string resource)>()).Value;
        
        // Act
        Result result = role.RemovePermission("", "USERS");
        
        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("INVALID_PERMISSION_SPEC");
    }

    [Fact]
    public void Test_AddPermissionsSucceeds_WhenValidPermissionsProvided()
    {
        // Arrange
        var role = RoleDomain.Role.Create(UserRole.Admin, new List<(string action, string resource)>()).Value;
        var permissions = new List<(string action, string resource)>
        {
            ("READ", "USERS"),
            ("WRITE", "POSTS"),
            ("DELETE", "COMMENTS")
        };
        
        // Act
        Result result = role.AddPermissions(permissions);
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        role.Permissions.Count.ShouldBe(3);
        role.Permissions.Any(p => p is { Action: "READ", Resource: "USERS" }).ShouldBeTrue();
        role.Permissions.Any(p => p is { Action: "WRITE", Resource: "POSTS" }).ShouldBeTrue();
        role.Permissions.Any(p => p is { Action: "DELETE", Resource: "COMMENTS" }).ShouldBeTrue();
    }

    [Fact]
    public void Test_AddPermissionsFailsFast_WhenInvalidPermissionInBatch()
    {
        // Arrange
        var role = RoleDomain.Role.Create(UserRole.Admin, new List<(string action, string resource)>()).Value;
        var permissions = new List<(string action, string resource)>
        {
            ("READ", "USERS"),
            ("", "POSTS"), // Invalid permission
            ("DELETE", "COMMENTS") // This should not be processed
        };
        
        // Act
        Result result = role.AddPermissions(permissions);
        
        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("INVALID_PERMISSION_SPEC");
        role.Permissions.Count.ShouldBe(0); // No permissions should be added due to fail-fast
    }

    [Fact]
    public void Test_RemovePermissionsSucceeds_WhenValidPermissionsProvided()
    {
        // Arrange
        var role = RoleDomain.Role.Create(UserRole.Admin, new List<(string action, string resource)>()).Value;
        role.AddPermissions(new List<(string action, string resource)>
        {
            ("READ", "USERS"),
            ("WRITE", "POSTS"),
            ("DELETE", "COMMENTS")
        });
        
        var permissionsToRemove = new List<(string action, string resource)>
        {
            ("READ", "USERS"),
            ("WRITE", "POSTS")
        };
        
        // Act
        Result result = role.RemovePermissions(permissionsToRemove);
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        role.Permissions.Count.ShouldBe(1);
        role.Permissions.First().Action.ShouldBe("DELETE");
        role.Permissions.First().Resource.ShouldBe("COMMENTS");
    }

    [Fact]
    public void Test_RemovePermissionsFailsFast_WhenPermissionNotFoundInBatch()
    {
        // Arrange
        var role = RoleDomain.Role.Create(UserRole.Admin, new List<(string action, string resource)>()).Value;
        role.AddPermission("READ", "USERS");
        
        var permissionsToRemove = new List<(string action, string resource)>
        {
            ("READ", "USERS"),
            ("WRITE", "POSTS") // This permission doesn't exist
        };
        
        // Act
        Result result = role.RemovePermissions(permissionsToRemove);
        
        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("PERMISSION_NOT_FOUND");
        role.Permissions.Count.ShouldBe(1); // READ/USERS should still be there due to fail-fast
    }

    
}