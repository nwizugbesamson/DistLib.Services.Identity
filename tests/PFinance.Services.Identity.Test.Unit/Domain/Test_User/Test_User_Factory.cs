using DistLib;
using NSubstitute;
using UserDomain = PFinance.Services.Identity.Domain.User;
using PFinance.Services.Identity.Common.Enums;
using Shouldly;

namespace PFinance.Services.Identity.Test.Unit.Domain.Test_User;

public class Test_User_Factory
{
    private readonly UserDomain.UserFactory _userFactory = new();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();

    [Theory]
    [InlineData("test.user@pfinance.com", "pass123", UserRole.Admin)]
    [InlineData("test.user2@pfinance.com", "pass123", UserRole.Customer)]
    [InlineData("admin@company.com", "securePass456", UserRole.Admin)]
    [InlineData("customer@example.org", "userPass789", UserRole.Customer)]
    public void Test_UserWithValidEmailIsCreatedSuccessfully(string email, string passwordHash, UserRole userRole)
    {
        // Arrange
        _dateTimeProvider.UtcNow.Returns(DateTime.Parse("2025-02-01 00:00:00"));
        
        // Act
        Result<UserDomain.User> result = _userFactory.CreateUser(email, passwordHash, userRole, _dateTimeProvider.UtcNow);
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldNotBeNull();
        result.Value.Email.Value.ShouldBe(email);
        result.Value.PasswordHash.ShouldBe(passwordHash);
        result.Value.Role.ShouldBe(userRole);
        result.Value.IsActive.ShouldBeTrue();
        result.Value.CreatedAt.ShouldBe(_dateTimeProvider.UtcNow);
    }

    [Theory]
    [InlineData("invalid-email", "pass123", UserRole.Admin)]
    [InlineData("test@", "pass123", UserRole.Customer)]
    [InlineData("@domain.com", "pass123", UserRole.Admin)]
    [InlineData("test..user@domain.com", "pass123", UserRole.Customer)]
    [InlineData("test@domain..com", "pass123", UserRole.Admin)]
    [InlineData("", "pass123", UserRole.Customer)]
    [InlineData(null, "pass123", UserRole.Admin)]
    public void Test_UserWithInvalidEmailIsNotCreated(string email, string passwordHash, UserRole userRole)
    {
        // Arrange
        _dateTimeProvider.UtcNow.Returns(DateTime.Parse("2025-02-01 00:00:00"));
        
        // Act
        Result<UserDomain.User> result = _userFactory.CreateUser(email, passwordHash, userRole, _dateTimeProvider.UtcNow);
        
        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
    }


    

    [Theory]
    [InlineData("test.user@pfinance.com", "", UserRole.Admin)]
    [InlineData("test.user2@pfinance.com", null, UserRole.Customer)]
    public void Test_UserCannotBeCreatedWithEmptyOrNullPasswordHash(string email, string passwordHash, UserRole userRole)
    {
        // Arrange
        _dateTimeProvider.UtcNow.Returns(DateTime.Parse("2025-02-01 00:00:00"));
        
        // Act
        Result<UserDomain.User> result = _userFactory.CreateUser(email, passwordHash, userRole, _dateTimeProvider.UtcNow);
        
        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.Code.ShouldContain("INVALID_PASSWORD");
    }

    [Fact]
    public void Test_UserFactoryImplementsIUserFactory()
    {
        // Arrange & Act
        var factory = new UserDomain.UserFactory();
        
        // Assert
        factory.ShouldBeAssignableTo<UserDomain.IUserFactory>();
    }
}