using DistLib;
using DistLib.Domain;
using NSubstitute;
using UserDomain = PFinance.Services.Identity.Domain.User;
using PFinance.Services.Identity.Common.Enums;
using Shouldly;

namespace PFinance.Services.Identity.Test.Unit.Domain.Test_User;

public class Test_User_Aggregate
{
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    [Theory]
    [InlineData("test.user@pfinance.com", "pass123", UserRole.Admin)]
    [InlineData("test.user2@pfinance.com", "pass123", UserRole.Customer)]
    public void Test_UserIsCreatedSuccessfully_WhenDataIsValid(string email, string passwordHash, UserRole userRole)
    {
        // Arrange
        Email testEmail = Email.Create(email).Value;
        
        _dateTimeProvider.UtcNow.Returns(DateTime.Parse("2025-02-01 00:00:00"));
        
        // Act
        UserDomain.User createdUser = UserDomain.User.Create(testEmail, passwordHash, userRole, _dateTimeProvider.UtcNow);
        
        // Assert
        createdUser.Id.ShouldNotBeNull();
        createdUser.Email.ShouldBe(testEmail);
        createdUser.PasswordHash.ShouldBe(passwordHash);
        createdUser.Role.ShouldBe(userRole);
        createdUser.CreatedAt.ShouldBe(_dateTimeProvider.UtcNow);
        
    }

    [Theory]
    [InlineData("test.user@pfinance.com", "pass123", UserRole.Admin)]
    [InlineData("test.user2@pfinance.com", "pass123", UserRole.Customer)]
    public void Test_CreatedUserIsActivatedSuccessfully_WhenDataIsValid(string email, string passwordHash, UserRole userRole)
    {
        // Arrange
        Email testEmail = Email.Create(email).Value;
        
        _dateTimeProvider.UtcNow.Returns(DateTime.Parse("2025-02-01 00:00:00"));
        
        // Act
        UserDomain.User createdUser = UserDomain.User.Create(testEmail, passwordHash, userRole, _dateTimeProvider.UtcNow);
        
        // Assert
        createdUser.IsActive.ShouldBe(true);
    }
    
    [Theory]
    [InlineData("test.user@pfinance.com", "pass123", UserRole.Admin)]
    [InlineData("test.user2@pfinance.com", "pass123", UserRole.Customer)]
    public void Test_UserCreation_GeneratedCreatedEvent_WhenSuccessful(string email, string passwordHash, UserRole userRole)
    {
        // Arrange
        Email testEmail = Email.Create(email).Value;
        
        _dateTimeProvider.UtcNow.Returns(DateTime.Parse("2025-02-01 00:00:00"));
        
        // Act
        UserDomain.User createdUser = UserDomain.User.Create(testEmail, passwordHash, userRole, _dateTimeProvider.UtcNow);
        
        // Assert
        createdUser.Events.Count.ShouldBe(1);
        createdUser.Events.Any(e => e is UserDomain.UserCreated).ShouldBeTrue();
    }
    
    // test user can be deactivated successfully when active
    [Theory]
    [InlineData("test.user@pfinance.com", "pass123", UserRole.Admin)]
    [InlineData("test.user2@pfinance.com", "pass123", UserRole.Customer)]
    public void Test_UserCanBeDeactivatedSuccessfully_WhenActive(string email, string passwordHash, UserRole userRole)
    {
        // Arrange
        Email testEmail = Email.Create(email).Value;
        
        _dateTimeProvider.UtcNow.Returns(DateTime.Parse("2025-02-01 00:00:00"));
        
        // Act
        UserDomain.User createdUser = UserDomain.User.Create(testEmail, passwordHash, userRole, _dateTimeProvider.UtcNow);
        Result result = createdUser.Deactivate();
        
        // Assert
        createdUser.IsActive.ShouldBe(false);
        result.IsSuccess.ShouldBeTrue();
        createdUser.Events.Count.ShouldBe(2);
        createdUser.Events.Any(e => e is UserDomain.UserDeactivated).ShouldBeTrue();
    }
    
    // test deactivation fails if user is already inactive
    [Theory]
    [InlineData("test.user@pfinance.com", "pass123", UserRole.Admin)]
    [InlineData("test.user2@pfinance.com", "pass123", UserRole.Customer)]
    public void Test_UserDeactivationFails_WhenUserIsNotActive(string email, string passwordHash, UserRole userRole)
    {// Arrange
        Email testEmail = Email.Create(email).Value;
        
        _dateTimeProvider.UtcNow.Returns(DateTime.Parse("2025-02-01 00:00:00"));
        
        // Act
        UserDomain.User createdUser = UserDomain.User.Create(testEmail, passwordHash, userRole, _dateTimeProvider.UtcNow);
        createdUser.Deactivate();
        Result result = createdUser.Deactivate();
        
        // Assert
        createdUser.IsActive.ShouldBe(false);
        createdUser.Events.Count.ShouldBe(2);
        result.IsSuccess.ShouldBeFalse();
        result.Error.Code.ShouldBe("ACCOUNT_ALREADY_DEACTIVATED");
        
    }
    
    [Theory]
    [InlineData("test.user@pfinance.com", "pass123", UserRole.Admin)]
    [InlineData("test.user2@pfinance.com", "pass123", UserRole.Customer)]
    public void Test_UserSigninConformationFails_WhenUserIsDeactivated(string email, string passwordHash, UserRole userRole)
    {
        // Arrange
        Email testEmail = Email.Create(email).Value;
        
        _dateTimeProvider.UtcNow.Returns(DateTime.Parse("2025-02-01 00:00:00"));
        
        // Act
        UserDomain.User createdUser = UserDomain.User.Create(testEmail, passwordHash, userRole, _dateTimeProvider.UtcNow);
        createdUser.Deactivate();
        Result result = createdUser.ConfirmSignIn();
        
        // Assert
        result.Error.Code.ShouldContain("INACTIVE_ACCOUNT");
        result.IsSuccess.ShouldBeFalse();
    }
    
    // test signin successful on active accounts
    [Theory]
    [InlineData("test.user@pfinance.com", "pass123", UserRole.Admin)]
    [InlineData("test.user2@pfinance.com", "pass123", UserRole.Customer)]
    public void Test_UserSigninConformationSucceeds_WhenUserIsValid(string email, string passwordHash, UserRole userRole)
    {
        // Arrange
        Email testEmail = Email.Create(email).Value;
        
        _dateTimeProvider.UtcNow.Returns(DateTime.Parse("2025-02-01 00:00:00"));
        
        // Act
        UserDomain.User createdUser = UserDomain.User.Create(testEmail, passwordHash, userRole, _dateTimeProvider.UtcNow);
        Result result = createdUser.ConfirmSignIn();
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        createdUser.Events.Count.ShouldBe(2);
        createdUser.Events.Any(e => e is UserDomain.UserSignedIn).ShouldBeTrue();
    }
}