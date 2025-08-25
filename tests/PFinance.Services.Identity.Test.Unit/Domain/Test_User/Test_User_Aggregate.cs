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
        createdUser.Email.ShouldBe(testEmail);
        createdUser.PasswordHash.ShouldBe(passwordHash);
        createdUser.Role.ShouldBe(userRole);
        createdUser.CreatedAt.ShouldBe(_dateTimeProvider.UtcNow);
        
    }
}